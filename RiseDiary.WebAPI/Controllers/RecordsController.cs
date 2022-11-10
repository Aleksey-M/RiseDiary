using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.WebAPI.Shared.Dto;

namespace RiseDiary.Api;

[ApiController]
[Route("api/records")]
public sealed class RecordsController : ControllerBase
{
    private readonly IRecordsService _recordService;

    private readonly ICogitationsService _cogitationsService;

    private readonly IAppSettingsService _appSettingsService;

    private readonly IRecordsThemesService _recordsThemesService;

    public RecordsController(
        IRecordsService recordService,
        IAppSettingsService appSettingsService,
        IRecordsThemesService recordsThemesService,
        ICogitationsService cogitationsService)
    {
        _recordService = recordService;
        _appSettingsService = appSettingsService;
        _recordsThemesService = recordsThemesService;
        _cogitationsService = cogitationsService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRecord(CreateRecordDto createDto)
    {
        var recId = await _recordService.AddRecord(createDto.Date, createDto.RecordName, createDto.RecordText);
        if (createDto.ThemesIds.Length > 0) await _recordsThemesService.AddRecordTheme(recId, createDto.ThemesIds);
        var newRecordUri = $@"{await _appSettingsService.GetHostAndPort()}/api/records/{recId}";
        return Created(newRecordUri, recId);
    }

    [HttpGet, Route("{recordId}")]
    [ProducesResponseType(typeof(RecordDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecordDto>> GetRecord(Guid recordId, CancellationToken cancellationToken)
    {
        var record = await _recordService.FetchRecordById(recordId, cancellationToken);

        return new RecordDto
        {
            Id = record.Id,
            Date = record.Date,
            CreatedDate = record.CreateDate,
            ModifiedDate = record.ModifyDate,
            Name = record.Name,
            Text = record.Text,
            Cogitations = record.Cogitations
                .Select(c => new CogitationDto
                {
                    Id = c.Id,
                    CreateDate = c.Date,
                    Text = c.Text
                })
                .ToArray(),
            Themes = record.ThemesRefs
                .Select(rt => rt.Theme)
                .Select(t => new ThemeDto
                {
                    ThemeId = t!.Id,
                    ThemeName = t.ThemeName,
                    Actual = t.Actual
                })
                .ToArray(),
            Images = record.ImagesRefs
                .Select(ri => ri.Image)
                .Select(i => new ImageListItemDto
                {
                    Id = i!.Id,
                    Name = i.Name,
                    Width = i.Width,
                    Height = i.Height,
                    SizeKb = i.GetSizeKbString(),
                    Base64Thumbnail = i.GetBase64Thumbnail()
                })
                .ToArray()
        };
    }

    [HttpDelete, Route("{recordId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRecord(Guid recordId)
    {
        await _recordService.DeleteRecord(recordId);
        return NoContent();
    }

    [HttpPut, Route("{recordId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRecord(Guid recordId, UpdateRecordDto updateRecordDto)
    {
        if (recordId != updateRecordDto.Id) return BadRequest("Not consistent request");

        await _recordService.UpdateRecord(recordId, updateRecordDto.NewDate, updateRecordDto.NewName, updateRecordDto.NewText);
        return NoContent();
    }

    [HttpPost, Route("{recordId}/cogitations")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddCogitation(Guid recordId, CreateCogitationDto createCogitationDto)
    {
        if (recordId != createCogitationDto.RecordId) return BadRequest("Not consistent request");

        var cogId = await _cogitationsService.AddCogitation(recordId, createCogitationDto.Text);
        var recordUri = $@"{await _appSettingsService.GetHostAndPort()}/api/v1.0/records/{recordId}";
        return Created(recordUri, cogId);
    }

    [HttpPut, Route("{recordId}/cogitations/{cogitationId}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCogitation(Guid recordId, Guid cogitationId, UpdateCogitationDto updateCogitationDto)
    {
        if (recordId != updateCogitationDto.RecordId || cogitationId != updateCogitationDto.CogitationId) return BadRequest("Not consistent request");

        await _cogitationsService.UpdateCogitationText(cogitationId, updateCogitationDto.NewText);
        return NoContent();
    }

    [HttpDelete, Route("{recordId}/cogitations/{cogitationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCogitation(Guid recordId, Guid cogitationId)
    {
        var record = await _recordService.FetchRecordById(recordId);
        var hasCogitation = record.Cogitations.Any(c => c.Id == cogitationId);

        if (!hasCogitation)
        {
            return BadRequest();
        }

        await _cogitationsService.DeleteCogitation(cogitationId);
        return NoContent();
    }
}
