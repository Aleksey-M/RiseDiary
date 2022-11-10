using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebAPI.Shared.Dto;

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

    [HttpPost, Route("scale-down")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ScaleDownImage(ScaleDownDto scaleDownDto)
    {
        await _imagesEditService.ReduceImageSize(scaleDownDto.ImageId, scaleDownDto.NewImageBiggestSide);
        return Ok();
    }

    [HttpPost, Route("replace-image")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReplaceImage([FromForm] ReplaceImageDto replaceImageDto)
    {
        if (replaceImageDto.Image == null) return BadRequest("New image should be selected");

        await _imagesEditService.ReplaceImage(replaceImageDto.Image, replaceImageDto.ImageId);
        return Ok();
    }

    [HttpGet, Route("{imageId}/scaled-preview")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ScaledImagePreviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ScaledImagePreviewDto>> GetScaledPreview(Guid imageId)
    {
        var scaled = await _cropImageService.CreateScaledImagePreview(imageId);
        return new ScaledImagePreviewDto
        {
            Coefficient = scaled.Coefficient,
            ImageBase64String = Convert.ToBase64String(scaled.Image)
        };
    }

    [HttpPost, Route("crop-image")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CropImage(CropImageDto cropImageDto)
    {
        var rect = new Rectangle(cropImageDto.Left, cropImageDto.Top, cropImageDto.Width, cropImageDto.Height);
        await _cropImageService.CropImage(cropImageDto.ImageId, rect, cropImageDto.Coefficient);
        return Ok();
    }

    [HttpPost, Route("{imageId}/save-changes-as-new-image")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> SaveChangesAsNewImage(Guid imageId)
    {
        return await _imagesEditService.CreateNewImageFromChanged(imageId);
    }

    [HttpPost, Route("{imageId}/apply-changes")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyChanges(Guid imageId)
    {
        await _imagesEditService.ApplyChanges(imageId);
        return Ok();
    }

    [HttpPost, Route("{imageId}/discard-changes")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DiscardChanges(Guid imageId)
    {
        await _imagesEditService.DiscardChanges(imageId);
        return Ok();
    }
}
