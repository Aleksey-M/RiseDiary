using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebAPI.Shared.Dto;

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

    [HttpPost, Route("{recordId}/themes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddThemeToRecord(Guid recordId, UpdateRecordThemesDto themes)
    {
        if (recordId != themes.RecordId) return BadRequest("Not consistent request");

        try
        {
            await _recordsThemesService.AddRecordTheme(recordId, themes.ThemesIds);
            return Ok();
        }
        catch (ArgumentException exc)
        {
            return BadRequest(exc.Message);
        }
    }

    [HttpDelete, Route("{recordId}/themes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRecordTheme(Guid recordId, UpdateRecordThemesDto themes)
    {
        if (recordId != themes.RecordId) return BadRequest("Not consistent request");

        try
        {
            await _recordsThemesService.RemoveRecordTheme(recordId, themes.ThemesIds);
            return NoContent();
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }
}
