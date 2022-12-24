using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared.Records;

namespace RiseDiary.WebAPI.Controllers.RecordsArea;

[ApiController]
[Route("api/records")]
public sealed class RecordsThemesController : ControllerBase
{
    private readonly IRecordsThemesService _recordsThemesService;

    public RecordsThemesController(IRecordsThemesService recordsThemesService)
    {
        _recordsThemesService = recordsThemesService;
    }

    [HttpPatch("{recordId}/themes")]
    public async Task<IActionResult> UpdateRecordThemes(Guid recordId, UpdateRecordThemesDto themes)
    {
        if (recordId != themes.RecordId) return BadRequest("Not consistent request");

        await _recordsThemesService.UpdateRecordThemes(recordId, themes.ThemesIds);
        return Ok();
    }
}
