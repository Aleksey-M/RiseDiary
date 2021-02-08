using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Dto;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    [Authorize]
    public class EditImagesController : ControllerBase
    {
        private readonly IImagesEditService _imagesEditService;
        private readonly ICropImageService _cropImageService;

        public EditImagesController(IImagesEditService imagesEditService, ICropImageService cropImageService)
        {
            _imagesEditService = imagesEditService;
            _cropImageService = cropImageService;
        }

        [HttpPost, Route("api/v1.0/images/scale-down")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ScaleDownImage(ScaleDownDto scaleDownDto)
        {
            try
            {
                await _imagesEditService.ReduceImageSize(scaleDownDto.ImageId, scaleDownDto.NewImageBiggestSide);
                return Ok();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPost, Route("api/v1.0/images/replace-image")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReplaceImage([FromForm] ReplaceImageDto replaceImageDto)
        {
            if (replaceImageDto.Image == null) return BadRequest("New image should be selected");

            try
            {
                await _imagesEditService.ReplaceImage(replaceImageDto.Image, replaceImageDto.ImageId);
                return Ok();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpGet, Route("api/v1.0/images/{imageId}/scaled-preview")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ScaledImagePreviewDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ScaledImagePreviewDto>> GetScaledPreview(Guid imageId)
        {
            try
            {
                var scaled = await _cropImageService.CreateScaledImagePreview(imageId);
                var dto = new ScaledImagePreviewDto
                {
                    Coefficient = scaled.Coefficient,
                    ImageBase64String = Convert.ToBase64String(scaled.Image)
                };

                return Ok(dto);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPost, Route("api/v1.0/images/crop-image")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CropImage(CropImageDto cropImageDto)
        {
            try
            {
                var rect = new Rectangle(cropImageDto.Left, cropImageDto.Top, cropImageDto.Width, cropImageDto.Height);
                await _cropImageService.CropImage(cropImageDto.ImageId, rect, cropImageDto.Coefficient);
                return Ok();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPost, Route("api/v1.0/images/{imageId}/save-changes-as-new-image")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        public async Task<ActionResult<Guid>> SaveChangesAsNewImage(Guid imageId)
        {
            try
            {
                var newImageId = await _imagesEditService.CreateNewImageFromChanged(imageId);
                return Ok(newImageId);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPost, Route("api/v1.0/images/{imageId}/apply-changes")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ApplyChanges(Guid imageId)
        {
            try
            {
                await _imagesEditService.ApplyChanges(imageId);
                return Ok();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPost, Route("api/v1.0/images/{imageId}/discard-changes")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DiscardChanges(Guid imageId)
        {
            try
            {
                await _imagesEditService.DiscardChanges(imageId);
                return Ok();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }
    }
}
