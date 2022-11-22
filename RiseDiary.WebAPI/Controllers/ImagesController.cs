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

    public ImagesController(IImagesService imagesService, IRecordsImagesService recordsImagesService, IAppSettingsService appSettingsService)
    {
        _imagesService = imagesService;
        _recordsImagesService = recordsImagesService;
        _appSettingsService = appSettingsService;
    }

    [HttpGet("api/image-file/{id}")]
    public async Task<IActionResult> GetImageFile(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty) return BadRequest();

        var image = await _imagesService.FetchFullImageById(id, cancellationToken);
        return File(image, "image/jpeg");
    }

    [HttpGet("api/image-thumbnail/{id}")]
    public async Task<IActionResult> GetImageThumbnail(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty) return BadRequest();

        var image = await _imagesService.FetchImageById(id, cancellationToken);
        return File(image.Thumbnail, "image/jpeg");
    }

    [HttpPost("api/images")]
    public async Task<IActionResult> UploadImages([FromForm] UploadImageDto imageDto)
    {
        if (imageDto.Image == null) return BadRequest("Image file should be selected");
        if (imageDto.NewBiggestDimension < 100 || imageDto.NewBiggestDimension > 10000) return BadRequest("Dimension size should be between 100 and 10 000");

        var newImageId = await _imagesService.AddImage(imageDto.Image, imageDto.ImageName, imageDto.NewBiggestDimension);

        if (imageDto.TargetRecordId != null && imageDto.TargetRecordId != Guid.Empty)
        {
            await _recordsImagesService.AddRecordImage(imageDto.TargetRecordId.Value, newImageId);
        }

        var newImageUri = $@"{await _appSettingsService.GetHostAndPort()}/api/v1.0/images/{newImageId}";

        return Created(newImageUri, newImageId);
    }

    [HttpDelete("api/images/{id}")]
    public async Task<IActionResult> DeleteImage(Guid id)
    {
        await _imagesService.DeleteImage(id);
        return NoContent();
    }

    [HttpPut("api/images/{id}")]
    public async Task<IActionResult> UpdateImage(Guid id, UpdateImageDto updateImageDto)
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
