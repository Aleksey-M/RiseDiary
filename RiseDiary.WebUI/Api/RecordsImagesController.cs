using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using System;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    [Authorize]
    public sealed class RecordsImagesController : ControllerBase
    {
        private readonly IRecordsImagesService _recordImagesService;

        public RecordsImagesController(IRecordsImagesService recordImagesService)
        {
            _recordImagesService = recordImagesService;
        }

        [HttpPost, Route("api/v1.0/records/{recordId}/images/{imageId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddImageToRecord(Guid recordId, Guid imageId, [FromQuery] int? order)
        {
            try
            {
                await _recordImagesService.AddRecordImage(recordId, imageId, order);
                return Ok();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPut, Route("api/v1.0/records/{recordId}/images/{imageId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeRecordImageOrder(Guid recordId, Guid imageId, [FromQuery] int order)
        {
            try
            {
                await _recordImagesService.ChangeRecordImageOrder(recordId, imageId, order);
                return Ok();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpDelete, Route("api/v1.0/records/{recordId}/images/{imageId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteRecordImage(Guid recordId, Guid imageId)
        {
            try
            {
                await _recordImagesService.RemoveRecordImage(recordId, imageId);
                return NoContent();
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }
        }
    }
}
