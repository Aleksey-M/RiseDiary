using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Images;

namespace RiseDiary.Api;

[ApiController]
public sealed class ImagesController : ControllerBase
{
    private readonly IImagesService _imagesService;

    private readonly IRecordsImagesService _recordsImagesService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly ILogger<ImagesController> _logger;

    private readonly UploadImageDtoValidator _uploadValidator;

    public ImagesController(
        IImagesService imagesService,
        IRecordsImagesService recordsImagesService,
        IAppSettingsService appSettingsService,
        ILogger<ImagesController> logger,
        UploadImageDtoValidator uploadValidator)
    {
        _imagesService = imagesService;
        _recordsImagesService = recordsImagesService;
        _appSettingsService = appSettingsService;
        _logger = logger;
        _uploadValidator = uploadValidator;
    }

    [HttpGet("api/image-file/{id}")]
    public async Task<IActionResult> GetImageFile(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty) return BadRequest();

        var image = await _imagesService.FetchFullImageById(id, cancellationToken);
        var img = await _imagesService.FetchImageById(id, cancellationToken);
        return File(image, img.ContentType);
    }

    [HttpGet("api/image-thumbnail/{id}")]
    public async Task<IActionResult> GetImageThumbnail(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty) return BadRequest();

        var image = await _imagesService.FetchImageById(id, cancellationToken);
        return File(image.Thumbnail, "image/jpeg");
    }

    [HttpPost("api/images")]
    public async Task<IActionResult> UploadImage([FromForm] UploadImageDto imageDto, [FromForm] IFormFile newImage)
    {
        if (newImage == null) return BadRequest("Image file should be selected");
        _uploadValidator.ValidateAndThrow(imageDto);
       
        var newImageId = await _imagesService.AddImage(
            formFile: newImage,
            imageName: imageDto.ImageName,
            newBiggestDimensionSize: imageDto.NewBiggestDimension,
            cameraModel: imageDto.CameraModel,
            taken: imageDto.Taken,
            contentType: imageDto.ContentType ?? newImage.ContentType);

        _logger.LogInformation("New image created with Id = '{imageId}'", newImageId);

        if (imageDto.TargetRecordId != null && imageDto.TargetRecordId != Guid.Empty)
        {
            await _recordsImagesService.AddRecordImage(imageDto.TargetRecordId.Value, newImageId);
            _logger.LogInformation("Image with Id = '{imageId}' attached to record with Id = '{recordId}'", newImageId, imageDto.TargetRecordId );
        }

        var newImageUri = $@"{await _appSettingsService.GetHostAndPort()}/api/images/{newImageId}";

        return Created(newImageUri, newImageId);
    }

    [HttpDelete("api/images/{id}")]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        await _imagesService.DeleteImage(id);
        return NoContent();
    }

    [HttpPut("api/images/{id}")]
    public async Task<IActionResult> UpdateImage(Guid id, UpdateImageNameDto updateImageDto)
    {
        if (id != updateImageDto.ImageId) return BadRequest("Not consistent request");

        await _imagesService.UpdateImage(updateImageDto.ImageId, updateImageDto.ImageNewName);
        return NoContent();
    }

    [HttpGet("api/images/{id}")]
    public async Task<ActionResult<ImageDto>> GetImage(Guid id, CancellationToken cancellationToken)
    {
        var img = await _imagesService.FetchImageById(id, cancellationToken);
        var fullImg = await _imagesService.FetchFullImageById(id, cancellationToken);

        return new ImageDto
        {
            Id = img.Id,
            Name = img.Name,
            CreateDate = img.CreateDate,
            ModifyDate = img.ModifyDate,
            Width = img.Width,
            Height = img.Height,
            CameraModel = img.CameraModel ?? "",
            Taken = img.Taken,
            SizeKb = img.GetSizeKbString(),
            ContentType = img.ContentType,
            Base64Image = Convert.ToBase64String(fullImg),
            TempImage = img.TempImage == null ? null : new TempImageDto
            {
                Id = img.TempImage.Id,
                Modification = img.TempImage.Modification,
                Width = img.TempImage.Width,
                Height = img.TempImage.Height,
                SizeKb = img.TempImage.GetSizeKbString()
            }
        };
    }

    [HttpGet("api/images")]
    public async Task<ActionResult<ImagesPageDto>> GetImagesPage([FromQuery] int? pageNo,
        [FromQuery] string? imageNameFilter, CancellationToken cancellationToken)
    {
        var pageSize = await _appSettingsService.GetAppSettingInt(AppSettingsKey.ImagesPageSize) ?? 20;
        pageNo ??= 1;
        var count = await _imagesService.GetImagesCount(imageNameFilter, cancellationToken: cancellationToken);

        var pagesInfo = PagesInfo.GetPagesInfo(count, pageNo.Value, pageSize);

        var images = await _imagesService.FetchImageSet(
            pagesInfo.StartIndex, pagesInfo.PageSize, imageNameFilter, cancellationToken: cancellationToken);

        return new ImagesPageDto
        {
            PagesInfo = pagesInfo,
            Images = images.Select(i => new ImageListItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Width = i.Width,
                Height = i.Height,
                SizeKb = i.GetSizeKbString(),
                Base64Thumbnail = i.GetBase64Thumbnail()
            })
            .ToList()
        };
    }
}
