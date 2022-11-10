using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;

namespace RiseDiary.Api;

[ApiController]
[Route("api/records")]
public sealed class RecordsImagesController : ControllerBase
{
    private readonly IRecordsImagesService _recordImagesService;

    public RecordsImagesController(IRecordsImagesService recordImagesService)
    {
        _recordImagesService = recordImagesService;
    }

    [HttpPost, Route("{recordId}/images/{imageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddImageToRecord(Guid recordId, Guid imageId, [FromQuery] int? order)
    {
        await _recordImagesService.AddRecordImage(recordId, imageId, order);
        return Ok();
    }

    [HttpPut, Route("{recordId}/images/{imageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeRecordImageOrder(Guid recordId, Guid imageId, [FromQuery] int order)
    {
        await _recordImagesService.ChangeRecordImageOrder(recordId, imageId, order);
        return NoContent();
    }

    [HttpDelete, Route("{recordId}/images/{imageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRecordImage(Guid recordId, Guid imageId)
    {
        await _recordImagesService.RemoveRecordImage(recordId, imageId);
        return NoContent();
    }
}
