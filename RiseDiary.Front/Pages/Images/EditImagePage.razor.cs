using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RiseDiary.Front.AppServices;
using RiseDiary.Shared.Images;

namespace RiseDiary.Front.Pages.Images;

public partial class EditImagePage : UIComponentBase
{
    [Inject]
    public HttpClient Http { get; set; } = null!;

    [Inject]
    public ILogger<EditImagePage> Logger { get; set; } = null!;

    [Inject]
    public NavigationManager NavManager { get; set; } = null!;

    [Inject]
    public IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    public JsDependencies JsDeps { get; set; } = null!;

    [Inject]
    public ScaleDownImageDtoValidator ScaleImageValidator { get; set; } = null!;

    [Inject]
    public CropImageDtoValidator CropImageValidator { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery]
    public Guid ImageId { get; set; }

    [Parameter]
    [SupplyParameterFromQuery]
    public Guid? RedirectRecordId { get; set; }


    private ImageDto _imageDto = null!;

    private string ImageUri { get; set; } = string.Empty;


    private string GetFullRecordUri(Guid recordId) => NavManager.GetUriWithQueryParameters("records/view",
        new Dictionary<string, object?>
        {
            ["recordId"] = recordId.ToString()
        });

    private async Task LoadImage()
    {
        if (_jcropRef != null)
        {
            await _jcropRef.InvokeVoidAsync("detach");
            await _jcropRef.DisposeAsync();
            _jcropRef = null;
        }

        var dto = await Http.GetFromJsonAsync<ImageDto>($"api/images/{ImageId}");

        if (dto == null)
        {
            Logger.LogWarning("Image with Id = '{imageId}' does not exists", ImageId);
            Redirect();
        }

        _imageDto = dto!;

        // viewId - for reloading image after any changing
        ImageUri = NavManager.GetUriWithQueryParameters($"{NavManager.BaseUri}api/image-file/{ImageId}",
            new Dictionary<string, object?>
            {
                ["viewId"] = Guid.NewGuid()
            });
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (ImageId == default)
        {
            Logger.LogWarning("Image Id not passed");
            Redirect();
        }

        _moduleTask = new(() => JsRuntime.InvokeAsync<IJSObjectReference>(
           "import", "./Pages/Images/EditImagePage.razor.js").AsTask());

        try
        {
            await LoadImage();
            await FinishApiRequest(null);
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Error on Edit Iage Page loading");
            await FinishApiRequest(exc.Message);
        }
    }

    private void Redirect()
    {
        if (RedirectRecordId.HasValue)
        {
            NavManager.NavigateTo(NavManager.GetUriWithQueryParameters("records/view",
                new Dictionary<string, object?> { ["recordId"] = RedirectRecordId.Value.ToString() }));
        }

        NavManager.NavigateTo("images");
    }

    private async Task ReplaceFile(InputFileChangeEventArgs e)
    {
        var file = e.File;

        try
        {
            await StartApiRequest();

            var formContent = new MultipartFormDataContent
            {
                { new StreamContent(file.OpenReadStream(file.Size)), "newImage", file.Name },
                { new StringContent(file.ContentType), "ContentType" },
                { new StringContent(ImageId.ToString()), "ImageId" }
            };

            await Http.PostAsync($"api/images/{ImageId}/replace", formContent);

            await LoadImage();
            await FinishApiRequest(null);
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Error on replacing image. Id = '{imageId}'. New image file name = '{imageFileName}'", ImageId, file.Name);
            await FinishApiRequest(exc.Message);
        }
    }

    private async Task SaveUpdatedImage()
    {
        try
        {
            await StartApiRequest();

            await Http.PostAsync($"api/images/{ImageId}/apply-changes", null);

            await LoadImage();
            await FinishApiRequest(null);
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Error on saving image. Id = '{imageId}'", ImageId);
            await FinishApiRequest(exc.Message);
        }
    }

    private async Task SaveUpdatedAsNewImage()
    {
        try
        {
            await StartApiRequest();

            var dto = new SaveAsNewImageDto
            {
                ImageId = ImageId,
                RecordId = RedirectRecordId
            };

            var response = await Http.PostAsJsonAsync($"api/images/{ImageId}/save-as-new", dto);

            var responseContent = await GetCheckedResponseContent<string?>(response);

            if (!Guid.TryParse(responseContent, out var newImageId))
            {
                Logger.LogWarning("Неверный формат Id: {incorrectImageId}", responseContent);
                await FinishApiRequest($"Неверный формат Id: {responseContent}");
                return;
            }

            ImageId = newImageId;

            await LoadImage();
            await FinishApiRequest(null);
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Saving image error. Id = '{imageId}'", ImageId);
            await FinishApiRequest(exc.Message);
        }
    }

    private async Task CancelEdit()
    {
        try
        {
            await StartApiRequest();

            await Http.PostAsync($"api/images/{ImageId}/discard-changes", null);

            await LoadImage();
            await FinishApiRequest(null);
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Error on changes rollback. Id='{imageId}'", ImageId);
            await FinishApiRequest(exc.Message);
        }
    }

    private async Task DeleteImage()
    {
        try
        {
            var confirmed = await JsRuntime.InvokeAsync<bool>("confirm", "Удалить изображение?");
            if (!confirmed) return;

            await StartApiRequest();

            var response = await Http.DeleteAsync($"api/images/{ImageId}");
            if (await CheckSuccessResponse(response))
            {
                Redirect();
            }
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Image deleting error. Id='{imageId}'", ImageId);
            await FinishApiRequest(exc.Message);
        }
    }

    private async Task RotateImage(bool right)
    {
        try
        {
            await StartApiRequest();

            var dto = new RotateImageDto
            {
                ImageId = ImageId,
                RotateRight = right
            };

            await Http.PostAsJsonAsync($"api/images/{ImageId}/rotate", dto);

            await LoadImage();
            await FinishApiRequest(null);
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Image deleting error. Id='{imageId}'", ImageId);
            await FinishApiRequest(exc.Message);
        }
    }

    private async Task ScaleImage()
    {
        try
        {
            var biggestDim = _imageDto.GetBiggestDimension();
            var inputValue = await JsRuntime.InvokeAsync<string>("prompt", "Введите новый размер большей стороны картинки для сжатия", biggestDim - 1);

            if (!int.TryParse(inputValue, out int newDim))
            {
                Logger.LogInformation("Incorrect input value");
                return;
            }

            var dto = new ScaleDownImageDto
            {
                ImageId = ImageId,
                ImageMaxDimension = biggestDim,
                DimensionMaxLimit = newDim
            };

            var validationResult = ScaleImageValidator.Validate(dto);

            if (!validationResult.IsValid)
            {
                ErrorMessage = validationResult.ToString();
                return;
            }

            await StartApiRequest();

            await Http.PostAsJsonAsync($"api/images/{ImageId}/scale-down", dto);

            await LoadImage();
            await FinishApiRequest(null);
        }
        catch (Exception exc)
        {
            Logger.LogWarning(exc, "Image scaling error. Id='{imageId}'", ImageId);
            await FinishApiRequest(exc.Message);
        }
    }

    private IJSObjectReference? _jcropRef;

    private string ScissorsIcon => _jcropRef == null ? "img/scissors.svg" : "img/scissors-red.svg";

    public async Task CropImage()
    {
        if (_imageDto.TempImage == null)
        {
            if (_jcropRef == null)
            {
                _jcropRef = await InitJcropLibrary("image-view");
                return;
            }

            var selection = await _jcropRef.InvokeAsync<ImageSelection>("getSelection");
            if (selection == null || (selection.SelWidth <= 0 || selection.SelHeight <= 0))
            {
                ErrorMessage = "Не выбрана область изображения для обрезки";
                return;
            }

            try
            {
                var dto = selection.CreateDto(ImageId);

                var validationResult = CropImageValidator.Validate(dto);

                if (!validationResult.IsValid)
                {
                    ErrorMessage = validationResult.ToString();
                    return;
                }

                await StartApiRequest();

                await Http.PostAsJsonAsync($"api/images/{ImageId}/crop", dto);

                await LoadImage();
                await FinishApiRequest(null);
            }
            catch (Exception exc)
            {
                Logger.LogWarning(exc, "Crop image error. Id='{imageId}'", ImageId);
                await FinishApiRequest(exc.Message);
            }
        }
    }

    private Lazy<Task<IJSObjectReference>> _moduleTask = null!;

    public async Task<IJSObjectReference> InitJcropLibrary(string imageElementId)
    {
        await JsDeps.InitJcrop();
        var module = await _moduleTask.Value;

        var jcropRef = await module.InvokeAsync<IJSObjectReference>("JcropInstance");
        if (jcropRef == null) throw new Exception("Can't create jcrop js instance");

        await jcropRef.InvokeVoidAsync("attach", imageElementId);

        return jcropRef;
    }

    protected override async ValueTask DisposeComponent()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;

            if (_jcropRef != null)
            {
                try
                {
                    await _jcropRef.InvokeVoidAsync("detach");
                    await _jcropRef.DisposeAsync();
                    _jcropRef = null;
                }
                finally
                {
                    _jcropRef = null;
                }
            }

            await module.DisposeAsync();
        }
    }
}
