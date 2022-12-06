using System.Drawing;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Images;

namespace RiseDiary.WebAPI.Controllers.ImagesArea;

[ApiController]
[Route("api/images")]
public sealed class EditImagesController : ControllerBase
{
    private readonly IImagesEditService _imagesEditService;

    private readonly ILogger<EditImagesController> _logger;

    private readonly IRecordsImagesService _recordsImagesService;

    public EditImagesController(
        IImagesEditService imagesEditService,
        IRecordsImagesService recordsImagesService,
        ILogger<EditImagesController> logger)
    {
        _imagesEditService = imagesEditService;
        _logger = logger;
        _recordsImagesService = recordsImagesService;
    }

    [HttpPost("{id}/scale-down")]
    public async Task<IActionResult> ScaleDownImage(Guid id, ScaleDownImageDto dto)
    {
        if (id != dto.ImageId)
        {
            _logger.LogWarning("Not consistent request. Request image Id='{requestImageId}'. Dto image Id='{dtoImageId}'",
                id, dto.ImageId);

            return BadRequest("Not consistent request");
        }

        await _imagesEditService.ReduceImageSize(dto.ImageId, dto.DimensionMaxLimit);
        return Ok();
    }

    [HttpPost("{id}/replace")]
    public async Task<IActionResult> ReplaceImage(Guid id, [FromForm] ReplaceImageDto replaceImageDto, [FromForm] IFormFile newImage)
    {
        if (newImage == null)
        {
            _logger.LogWarning("New image does not selected for replacing");
            return BadRequest("New image does not selected for replacing");
        }

        if (id != replaceImageDto.ImageId)
        {
            _logger.LogWarning("Not consistent request. Request image Id='{requestImageId}'. Dto image Id='{dtoImageId}'",
                id, replaceImageDto.ImageId);

            return BadRequest("Not consistent request");
        }

        await _imagesEditService.ReplaceImage(newImage, replaceImageDto.ImageId, replaceImageDto.ContentType);
        _logger.LogInformation("Image replaced. Id = '{imageId}'", replaceImageDto.ImageId);

        return Ok();
    }

    [HttpPost("{id}/crop")]
    public async Task<IActionResult> CropImage(Guid id, CropImageDto dto, [FromServices] CropImageDtoValidator validator)
    {
        if (id != dto.ImageId)
        {
            _logger.LogWarning("Not consistent request. Request image Id='{requestImageId}'. Dto image Id='{dtoImageId}'",
                id, dto.ImageId);

            return BadRequest("Not consistent request");
        }

        validator.ValidateAndThrow(dto);

        var rect = new Rectangle(dto.Left, dto.Top, dto.Width, dto.Height);

        await _imagesEditService.CropImage(dto.ImageId, rect);

        return Ok();
    }

    [HttpPost("{id}/save-as-new")]
    public async Task<IActionResult> SaveChangesAsNewImage(Guid id, SaveAsNewImageDto dto)
    {
        if (id != dto.ImageId)
        {
            _logger.LogWarning("Not consistent request. Request image Id='{requestImageId}'. Dto image Id='{dtoImageId}'",
                id, dto.ImageId);

            return BadRequest("Not consistent request");
        }

        var newImageId = await _imagesEditService.CreateNewImageFromChanged(dto.ImageId);

        if (dto.RecordId.HasValue && dto.RecordId.Value != default)
        {
            await _recordsImagesService.AddRecordImage(dto.RecordId.Value, newImageId);
        }

        var newImageUri = $@"api/images/{newImageId}";

        return Created(newImageUri, newImageId);
    }

    [HttpPost("{imageId}/apply-changes")]
    public async Task<IActionResult> ApplyChanges(Guid imageId)
    {
        await _imagesEditService.ApplyChanges(imageId);
        return Ok();
    }

    [HttpPost("{imageId}/discard-changes")]
    public async Task<IActionResult> DiscardChanges(Guid imageId)
    {
        await _imagesEditService.DiscardChanges(imageId);
        return Ok();
    }

    [HttpPost("{id}/rotate")]
    public async Task<IActionResult> Rotate(Guid id, RotateImageDto dto)
    {
        if (id != dto.ImageId)
        {
            _logger.LogWarning("Not consistent request. Request image Id='{requestImageId}'. Dto image Id='{dtoImageId}'",
                id, dto.ImageId);

            return BadRequest("Not consistent request");
        }

        await _imagesEditService.RotateImage(dto.ImageId, dto.RotateRight ? Turn.Right : Turn.Left);

        return Ok();
    }
}
