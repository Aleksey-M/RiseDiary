using ExifLibrary;
using Microsoft.AspNetCore.Components.Forms;
using RiseDiary.Front.JsComponents;
using RiseDiary.Shared.Images;

namespace RiseDiary.Front.AppServices;

public class PrepareImageService
{
    private protected ImageDimensionsReader _jsImgHelper;

    private readonly ILogger<PrepareImageService> _logger;

    public PrepareImageService(ImageDimensionsReader jsImgHelper, ILogger<PrepareImageService> logger)
    {
        _logger = logger;
        _jsImgHelper = jsImgHelper;
    }

    public UploadImageDto CreateUploadDto(
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

    public async Task<Stream> PrepareImage(IBrowserFile imgFile, UploadImageDto dto)
    {
        if (dto.NewBiggestDimension.HasValue)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Loading image file '{fileName}' to memory. File size: {fileSize}", imgFile.Name, imgFile.Size);

            var memoryStream = new MemoryStream();
            using var readStream = imgFile.OpenReadStream(imgFile.Size);
            await readStream.CopyToAsync(memoryStream);

            memoryStream.Position = 0;
            var imgDimensions = await _jsImgHelper.GetImgDimensions(memoryStream);

            if (dto.NewBiggestDimension.Value < imgDimensions?.GetBiggest())
            {
                _logger.LogInformation(
                    "Reading metadata from file '{fileName}' on the client. File size: {fileSize}.",
                    imgFile.Name, imgFile.Size);

                memoryStream.Position = 0;
                (dto.Taken, dto.CameraModel) = GetMetadataFromPhoto(memoryStream);


                _logger.LogInformation(
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

            _logger.LogInformation(
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
            _logger.LogWarning(exc, "Error on reading metadata from image file");
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

    public MultipartFormDataContent PrepareFormContent(Stream imageStream, UploadImageDto dto)
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
