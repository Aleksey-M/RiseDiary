using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Images;
using RiseDiary.WebUI.Model;

namespace RiseDiary.WebAPI.Controllers.ImagesArea;

[ApiController]
public sealed class ImagesController : ControllerBase
{
    private readonly IImagesService _imagesService;

    private readonly IRecordsImagesService _recordsImagesService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly ILogger<ImagesController> _logger;

    public ImagesController(
        IImagesService imagesService,
        IRecordsImagesService recordsImagesService,
        IAppSettingsService appSettingsService,
        ILogger<ImagesController> logger)
    {
        _imagesService = imagesService;
        _recordsImagesService = recordsImagesService;
        _appSettingsService = appSettingsService;
        _logger = logger;
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
    public async Task<IActionResult> UploadImage([FromForm] UploadImageDto imageDto,
        [FromForm] IFormFile newImage, UploadImageDtoValidator uploadValidator)
    {
        if (newImage == null) return BadRequest("Image file should be selected");

        uploadValidator.ValidateAndThrow(imageDto);

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
            _logger.LogInformation("Image with Id = '{imageId}' attached to record with Id = '{recordId}'", newImageId, imageDto.TargetRecordId);
        }

        var newImageUri = $@"{Request.GetAppBaseUrl()}/api/images/{newImageId}";

        return Created(newImageUri, newImageId);
    }

    [HttpDelete("api/images/{id}")]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        await _imagesService.DeleteImage(id);
        return NoContent();
    }

    [HttpPut("api/images/{id}")]
    public async Task<IActionResult> UpdateImage(Guid id, UpdateImageNameDto dto,
        [FromServices] UpdateImageNameDtoValidator validator)
    {
        if (id != dto.ImageId) return BadRequest("Not consistent request");

        validator.ValidateAndThrow(dto);

        await _imagesService.UpdateImage(dto.ImageId, dto.ImageName);
        return NoContent();
    }

    [HttpGet("api/images/{id}")]
    public async Task<ActionResult<ImageDto>> GetImage(Guid id, CancellationToken cancellationToken)
    {
        var img = await _imagesService.FetchImageById(id, cancellationToken);
        var imageLinks = await _recordsImagesService.GetLinkedRecordsInfo(id, cancellationToken);

        return img.ToDto(imageLinks);
    }

    [HttpGet("api/images")]
    public async Task<ActionResult<ImagesPageDto>> GetImagesPage(
        [FromQuery] int? pageNo,
        [FromQuery] string? imageNameFilter,
        [FromQuery] Guid? recordId, 
        CancellationToken cancellationToken)
    {
        var pageSize = await _appSettingsService.GetAppSettingInt(AppSettingsKey.ImagesPageSize) ?? 20;
        pageNo ??= 1;

        var count = await _imagesService.GetImagesCount(imageNameFilter, recordId, cancellationToken: cancellationToken);

        var pagesInfo = PagesInfo.GetPagesInfo(count, pageNo.Value, pageSize);

        var images = await _imagesService.FetchImageSet(pagesInfo.StartIndex,
            pagesInfo.PageSize, imageNameFilter, recordId, cancellationToken: cancellationToken);

        return new ImagesPageDto
        {
            PagesInfo = pagesInfo,
            Images = images.Select(x => x.ToListDto()).ToList()
        };
    }
}
