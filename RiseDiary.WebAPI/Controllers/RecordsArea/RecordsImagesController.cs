using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;

namespace RiseDiary.WebAPI.Controllers.RecordsArea;

[ApiController]
[Route("api/records")]
public sealed class RecordsImagesController : ControllerBase
{
    private readonly IRecordsImagesService _recordImagesService;

    public RecordsImagesController(IRecordsImagesService recordImagesService)
    {
        _recordImagesService = recordImagesService;
    }

    [HttpPost("{recordId}/images/{imageId}")]
    public async Task<IActionResult> AddImageToRecord(Guid recordId, Guid imageId, [FromQuery] int? order)
    {
        await _recordImagesService.AddRecordImage(recordId, imageId, order);
        return Ok();
    }

    [HttpPut("{recordId}/images/{imageId}")]
    public async Task<IActionResult> ChangeRecordImageOrder(Guid recordId, Guid imageId, [FromQuery] int order)
    {
        await _recordImagesService.ChangeRecordImageOrder(recordId, imageId, order);
        return NoContent();
    }

    [HttpDelete("{recordId}/images/{imageId}")]
    public async Task<IActionResult> DeleteRecordImage(Guid recordId, Guid imageId)
    {
        await _recordImagesService.RemoveRecordImage(recordId, imageId);
        return NoContent();
    }
}
