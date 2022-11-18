using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Dto;

namespace RiseDiary.Api;

[ApiController]
[Route("api/records")]
public sealed class RecordsThemesController : ControllerBase
{
    private readonly IRecordsThemesService _recordsThemesService;

    public RecordsThemesController(IRecordsThemesService recordsThemesService)
    {
        _recordsThemesService = recordsThemesService;
    }

    [HttpPost("{recordId}/themes")]
    public async Task<IActionResult> AddThemeToRecord(Guid recordId, UpdateRecordThemesDto themes)
    {
        if (recordId != themes.RecordId) return BadRequest("Not consistent request");

        await _recordsThemesService.AddRecordTheme(recordId, themes.ThemesIds);
        return Ok();
    }

    [HttpDelete("{recordId}/themes")]
    public async Task<IActionResult> DeleteRecordTheme(Guid recordId, UpdateRecordThemesDto themes)
    {
        if (recordId != themes.RecordId) return BadRequest("Not consistent request");

        await _recordsThemesService.RemoveRecordTheme(recordId, themes.ThemesIds);
        return NoContent();
    }
}
