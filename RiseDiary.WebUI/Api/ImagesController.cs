using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Dto;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    [Authorize]
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

        [HttpGet, Route("api/v1.0/image-file/{id}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetImageFile(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var image = await _imagesService.FetchFullImageById(id, cancellationToken);
                return File(image, "image/jpeg");
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499);
            }
        }

        [HttpGet, Route("api/v1.0/image-thumbnail/{id}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetImageThumbnail(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var image = await _imagesService.FetchImageById(id, cancellationToken);
                return File(image.Thumbnail, "image/jpeg");
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499);
            }
        }

        [HttpPost, Route("api/v1.0/images")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadImages([FromForm] UploadImageDto imageDto)
        {
            if (imageDto.Image == null) return BadRequest("Image file should be selected");
            if (imageDto.NewBiggestDimension < 100 || imageDto.NewBiggestDimension > 10000) return BadRequest("Dimension size should be between 100 and 10 000");

            try
            {
                var newImageId = await _imagesService.AddImage(imageDto.Image, imageDto.ImageName, imageDto.NewBiggestDimension);

                if (imageDto.TargetRecordId != null && imageDto.TargetRecordId != Guid.Empty)
                {
                    await _recordsImagesService.AddRecordImage(imageDto.TargetRecordId.Value, newImageId);
                }

                var newImageUri = $@"{await _appSettingsService.GetHostAndPort()}/api/v1.0/images/{newImageId}";

                return Created(newImageUri, newImageId);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpDelete, Route("api/v1.0/images/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteImage(Guid id)
        {
            await _imagesService.DeleteImage(id);
            return NoContent();
        }

        [HttpPut, Route("api/v1.0/images/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateImage(Guid id, UpdateImageDto updateImageDto)
        {
            if (id != updateImageDto.ImageId) return BadRequest("Not consistent request");

            try
            {
                await _imagesService.UpdateImage(updateImageDto.ImageId, updateImageDto.ImageNewName);
                return NoContent();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpGet, Route("api/v1.0/images/{id}")]
        [ProducesResponseType(typeof(ImageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ImageDto>> GetImage(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var img = await _imagesService.FetchImageById(id, cancellationToken);
                var fullImg = await _imagesService.FetchFullImageById(id, cancellationToken);

                var dto = new ImageDto
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

                return Ok(dto);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499);
            }
        }

        [HttpGet, Route("api/v1.0/images")]
        [ProducesResponseType(typeof(ImagesPageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ImagesPageDto>> GetImagesPage([FromQuery] int? pageSize,
            [FromQuery] int? pageNo, [FromQuery] string? imageNameFilter, CancellationToken cancellationToken)
        {
            try
            {
                pageSize ??= 20;
                pageNo ??= 1;
                var count = await _imagesService.GetImagesCount(imageNameFilter, cancellationToken: cancellationToken);
                pageSize = pageSize > 100 ? 100 : pageSize;

                var pagesInfo = PagesInfo.GetPagesInfo(count, pageNo.Value, pageSize.Value);
                var images = await _imagesService.FetchImageSet(
                    pagesInfo.StartIndex, pagesInfo.PageSize, imageNameFilter, cancellationToken: cancellationToken);

                var dto = new ImagesPageDto
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
                    }).ToList()
                };

                return Ok(dto);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499);
            }
        }
    }
}
