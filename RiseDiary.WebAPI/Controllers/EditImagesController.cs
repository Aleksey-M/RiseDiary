using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Dto;

namespace RiseDiary.Api;

[ApiController]
[Route("api/images")]
public sealed class EditImagesController : ControllerBase
{
    private readonly IImagesEditService _imagesEditService;

    private readonly ICropImageService _cropImageService;

    public EditImagesController(IImagesEditService imagesEditService, ICropImageService cropImageService)
    {
        _imagesEditService = imagesEditService;
        _cropImageService = cropImageService;
    }

    [HttpPost("scale-down")]
    public async Task<IActionResult> ScaleDownImage(ScaleDownDto scaleDownDto)
    {
        await _imagesEditService.ReduceImageSize(scaleDownDto.ImageId, scaleDownDto.NewImageBiggestSide);
        return Ok();
    }

    [HttpPost("replace-image")]
    public async Task<IActionResult> ReplaceImage([FromForm] ReplaceImageDto replaceImageDto)
    {
        if (replaceImageDto.Image == null) return BadRequest("New image should be selected");

        await _imagesEditService.ReplaceImage(replaceImageDto.Image, replaceImageDto.ImageId);
        return Ok();
    }

    [HttpGet("{imageId}/scaled-preview")]
    public async Task<ActionResult<ScaledImagePreviewDto>> GetScaledPreview(Guid imageId)
    {
        var scaled = await _cropImageService.CreateScaledImagePreview(imageId);
        return new ScaledImagePreviewDto
        {
            Coefficient = scaled.Coefficient,
            ImageBase64String = Convert.ToBase64String(scaled.Image)
        };
    }

    [HttpPost("crop-image")]
    public async Task<IActionResult> CropImage(CropImageDto cropImageDto)
    {
        var rect = new Rectangle(cropImageDto.Left, cropImageDto.Top, cropImageDto.Width, cropImageDto.Height);
        await _cropImageService.CropImage(cropImageDto.ImageId, rect, cropImageDto.Coefficient);
        return Ok();
    }

    [HttpPost("{imageId}/save-changes-as-new-image")]
    public async Task<ActionResult<Guid>> SaveChangesAsNewImage(Guid imageId)
    {
        return await _imagesEditService.CreateNewImageFromChanged(imageId);
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
}
