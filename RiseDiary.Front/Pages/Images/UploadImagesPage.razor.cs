using ExifLibrary;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RiseDiary.Shared.Images;

namespace RiseDiary.Front.Pages.Images;

public partial class UploadImagesPage : UIComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = null!;

    [Inject]
    public ILogger<UploadImagesPage> Logger { get; set; } = null!;

    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    [Inject]
    public UploadImageDtoValidator Validator { get; set; } = null!;

    [Inject]
    public IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery]
    public Guid? RecordId { get; set; }

    private readonly List<IBrowserFile> _loadedFiles = new();

    private const int FilesMaxCount = 10;
    private const long MaxFileSize = 1024 * 1024 * 15;

    private string? _imageName;
    private int? _biggestDimmension;

    private async Task SelectFiles(InputFileChangeEventArgs e)
    {
        Loading = true;
        _loadedFiles.Clear();
        ErrorMessage = null;

        try
        {
            foreach (var file in e.GetMultipleFiles(FilesMaxCount))
            {
                try
                {
                    _loadedFiles.Add(file);
                    StateHasChanged();
                    await Task.Yield();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Error on uploading file: {fileName}", file.Name);
                }
            }
        }
        catch (Exception exc)
        {
            ErrorMessage = exc.Message;
            Logger.LogWarning(exc, "Error on uploading: {Message}", exc.Message);
        }

        Loading = false;
    }

    private async Task Upload()
    {
        if (_loadedFiles.Count == 0)
        {
            ErrorMessage = "Файл изображения не выбран";
            Logger.LogWarning("Uploading images list is empty");
            return;
        }

        if (_loadedFiles.Any(x => x.Size > MaxFileSize))
        {
            ErrorMessage = "Размер загружаемых файлов не должен превышать 15Мб";
            Logger.LogWarning("Files sizes should be less then 15mb");
            return;
        }

        string? imageName = null;
        if (!string.IsNullOrWhiteSpace(_imageName))
        {
            imageName = _imageName.Trim();
        }

        await StartApiRequest();

        Guid newImageId = default;

        try
        {
            foreach (var (img, index) in _loadedFiles.Select((x, i) => (x, i)))
            {
                var uploadDto = CreateUploadDto(img, _imageName, index, _biggestDimmension, RecordId);
                var validationResult = Validator.Validate(uploadDto);

                if (!validationResult.IsValid)
                {
                    Logger.LogWarning("Validation error {validationResult}", validationResult.ToString());
                    await FinishApiRequest(validationResult.ToString());
                    break;
                };

                var preparedImageStream = await PrepareImage(img, uploadDto);

                var formData = PrepareFormContent(preparedImageStream, uploadDto);

                var response = await Http.PostAsync("api/images", formData);

                if (_loadedFiles.Count > 1)
                {
                    await CheckSuccessResponse(response);
                }
                else
                {
                    var responseContent = await GetCheckedResponseContent<string?>(response);
                    if (responseContent == null)
                    {
                        Logger.LogWarning("New image Id wosn't returned by api. File name: '{fileName}'", img.Name);
                    }

                    if (!Guid.TryParse(responseContent, out newImageId))
                    {
                        Logger.LogWarning("Returned new image Id has icorrect format. File name: '{fileName}'. Id: '{incorrectId}'", img.Name, responseContent);
                    }
                }
            }
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Error on adding image");
            await FinishApiRequest(exc.Message);
        }


        if (HasError) return;

        var redirectUri = ((_loadedFiles.Count == 1 && newImageId != default), (RecordId.HasValue && RecordId.Value != default)) switch
        {
            (true, _) => NavManager.GetUriWithQueryParameters("images/edit", new Dictionary<string, object?>
            {
                ["recordId"] = RecordId,
                ["imageId"] = newImageId
            }),
            (false, true) => NavManager.GetUriWithQueryParameters("records/view", new Dictionary<string, object?>
            {
                ["recordId"] = RecordId
            }),
            (false, false) => NavManager.GetUriWithQueryParameters("images", new Dictionary<string, object?>())
        };

        NavManager.NavigateTo(redirectUri);
    }

    private Lazy<Task<IJSObjectReference>> _moduleTask = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _moduleTask = new(() => JsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./Pages/Images/UploadImagesPage.razor.js").AsTask());
    }

    public class ImgDimensions
    {
        public int Width { get; set; }

        public int Height { get; set; }
    }

    private async Task<ImgDimensions> GetImgDimensions(Stream imageDataStream)
    {
        try
        {
            var module = await _moduleTask.Value;
            using var streamRef = new DotNetStreamReference(imageDataStream, true);

            return await module.InvokeAsync<ImgDimensions>("getImageDimensions", streamRef);
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Error on js function '{jsFunctionName}' call", "getImageDimensions");
            throw;
        }
    }

    protected override async ValueTask DisposeComponent()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }

    private static UploadImageDto CreateUploadDto(
        IBrowserFile imgFile,
        string? imageName,
        int fileIndex,
        int? biggestDimension,
        Guid? recordId)
    {
        var uploadDto = new UploadImageDto
        {
            ImageName = (string.IsNullOrWhiteSpace(imageName), fileIndex) switch
            {
                (true, _) => Path.GetFileNameWithoutExtension(imgFile.Name),
                (false, > 0) => $"{imageName!} ({fileIndex})",
                (false, _) => imageName!
            },
            ContentType = imgFile.ContentType,
            NewBiggestDimension = biggestDimension,
            TargetRecordId = recordId
        };

        return uploadDto;
    }

    private async Task<Stream> PrepareImage(IBrowserFile imgFile, UploadImageDto dto)
    {
        if (dto.NewBiggestDimension.HasValue)
        {
            var startTime = DateTime.UtcNow;
            Logger.LogInformation("Loading image file '{fileName}' to memory. File size: {fileSize}", imgFile.Name, imgFile.Size);

            var memoryStream = new MemoryStream();
            using var readStream = imgFile.OpenReadStream(imgFile.Size);
            await readStream.CopyToAsync(memoryStream);

            memoryStream.Position = 0;
            var imgDimensions = await GetImgDimensions(memoryStream);

            if (dto.NewBiggestDimension.Value < Math.Max(imgDimensions.Width, imgDimensions.Height))
            {
                Logger.LogInformation(
                    "Reading metadata from file '{fileName}' on the client. File size: {fileSize}.",
                    imgFile.Name, imgFile.Size);

                memoryStream.Position = 0;
                (dto.Taken, dto.CameraModel) = GetMetadataFromPhoto(memoryStream);


                Logger.LogInformation(
                    "Resizing image file '{fileName}' on the client. File dimensions: {fileDimensionsW}/{fileDimensionsH}",
                    imgFile.Name, imgDimensions.Width, imgDimensions.Height);

                var resizedImageFile = await imgFile.RequestImageFileAsync(
                    format: "image/jpeg",
                    maxWidth: dto.NewBiggestDimension.Value,
                    maxHeight: dto.NewBiggestDimension.Value);

                memoryStream.Close();
                await memoryStream.DisposeAsync();

                memoryStream = new MemoryStream();
                using var readStream2 = resizedImageFile.OpenReadStream(resizedImageFile.Size);
                await readStream2.CopyToAsync(memoryStream);

                dto.NewBiggestDimension = null;
                dto.ContentType = "image/jpeg";
            }

            Logger.LogInformation(
                "File processing finished. File name: '{fileName}'. Processing time in seconds: {processingTime}",
                imgFile.Name,
                (DateTime.UtcNow - startTime).TotalSeconds);

            memoryStream.Position = 0;
            return memoryStream;
        }

        return imgFile.OpenReadStream(imgFile.Size);
    }

    private (DateTime? taken, string? cameraModel) GetMetadataFromPhoto(MemoryStream stream)
    {
        ImageFile file;

        try
        {
            file = ImageFile.FromStream(stream);
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Error on reading metadata from image file");
            return (null, null);
        }

        var prop = file.Properties.FirstOrDefault(p => p.Tag == ExifTag.DateTimeOriginal);
        DateTime? taken = prop != null ? (DateTime)prop.Value : (DateTime?)null;

        var model = file.Properties.FirstOrDefault(p => p.Tag == ExifTag.Model)?.Value?.ToString() ?? "";
        var make = file.Properties.FirstOrDefault(p => p.Tag == ExifTag.Make)?.Value.ToString() ?? "";
        string? cameraModel = model.Contains(make, StringComparison.OrdinalIgnoreCase) ? model : make + " " + model;
        cameraModel = string.IsNullOrWhiteSpace(cameraModel) ? null : cameraModel.Trim();

        return (taken, cameraModel);
    }

    private static MultipartFormDataContent PrepareFormContent(Stream imageStream, UploadImageDto dto)
    {
        var formContent = new MultipartFormDataContent
        {
            { new StreamContent(imageStream), "newImage", dto.ImageName },
            { new StringContent(dto.ContentType ?? "image/jpeg"), nameof(dto.ContentType) },
            { new StringContent(dto.ImageName), nameof(dto.ImageName) }
        };

        if (dto.TargetRecordId.HasValue && dto.TargetRecordId.Value != default)
        {
            formContent.Add(new StringContent(dto.TargetRecordId.Value.ToString()), nameof(dto.TargetRecordId));
        }

        if (dto.NewBiggestDimension.HasValue)
        {
            formContent.Add(new StringContent(dto.NewBiggestDimension.Value.ToString()), nameof(dto.NewBiggestDimension));
        }

        if (!string.IsNullOrWhiteSpace(dto.CameraModel))
        {
            formContent.Add(new StringContent(dto.CameraModel), nameof(dto.CameraModel));
        }

        if (dto.Taken.HasValue && dto.Taken.Value != default)
        {
            formContent.Add(new StringContent(dto.Taken.Value.ToString()), nameof(dto.Taken));
        }

        return formContent;
    }
}
