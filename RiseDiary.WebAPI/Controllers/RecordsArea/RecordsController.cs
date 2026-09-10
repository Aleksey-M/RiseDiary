using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Common.Records;
using RiseDiary.Model;
using RiseDiary.WebAPI.Controllers.ScopesArea;
using RiseDiary.WebAPI.Extensions;
using RiseDiary.WebAPI.Scopes.Services;
using RiseDiary.WebAPI.Settings;
using RiseDiary.WebAPI.Settings.Model;

namespace RiseDiary.WebAPI.Controllers.RecordsArea;

[ApiController]
[Route("api/records")]
public sealed class RecordsController : ControllerBase
{
    private readonly IRecordsService _recordService;

    private readonly ICogitationsService _cogitationsService;

    private readonly ISettingsService _appSettingsService;

    public RecordsController(
        IRecordsService recordService,
        ISettingsService appSettingsService,
        ICogitationsService cogitationsService)
    {
        _recordService = recordService;
        _appSettingsService = appSettingsService;
        _cogitationsService = cogitationsService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecord(CreateRecordDto createDto, [FromServices] CreateRecordValidator createRecordValidator)
    {
        createRecordValidator.ValidateAndThrow(createDto);

        var recId = await _recordService.AddRecord(createDto.Date, createDto.Name, createDto.Text);

        var newRecordUri = $@"{Request.GetAppBaseUrl()}/api/records/{recId}";
        return Created(newRecordUri, recId);
    }

    [HttpGet("{recordId}")]
    public async Task<ActionResult<RecordEditDto>> GetRecord(
        Guid recordId,
        [FromServices] IScopesService scopeService,
        CancellationToken cancellationToken)
    {
        var record = await _recordService.FetchRecordById(recordId, cancellationToken);
        var dto = record.ToDto();

        var startRecords = (await _appSettingsService.GetSetting<BookmarksSettings>(cancellationToken)).Data?.Records ?? [];
        var scopes = await scopeService.GetScopes(null, cancellationToken);

        var addImagesPageSize = (await _appSettingsService.GetSetting<PagesSizesSettings>(cancellationToken)).Data?.AvailableImagesPageSize;

        return record.ToEditDto(startRecords, scopes.Select(s => s.ToDto()).ToList(), addImagesPageSize ?? 10);
    }

    [HttpDelete("{recordId}")]
    public async Task<IActionResult> DeleteRecord(Guid recordId)
    {
        await _recordService.DeleteRecord(recordId);
        return NoContent();
    }

    [HttpPatch("{recordId}")]
    public async Task<IActionResult> UpdateRecord(Guid recordId, UpdateRecordDto updateRecordDto, UpdateRecordValidator validator)
    {
        if (recordId != updateRecordDto.Id) return BadRequest("Not consistent request");

        validator.ValidateAndThrow(updateRecordDto);

        await _recordService.UpdateRecord(recordId, updateRecordDto.Date, updateRecordDto.Name, updateRecordDto.Text);
        return NoContent();
    }

    [HttpPost("{recordId}/cogitations")]
    public async Task<IActionResult> AddCogitation(
        Guid recordId,
        CreateCogitationDto dto,
        [FromServices] CreateCogitationValidator validator)
    {
        if (recordId != dto.RecordId) return BadRequest("Not consistent request");

        validator.ValidateAndThrow(dto);

        var cogId = await _cogitationsService.AddCogitation(recordId, dto.Text);

        var recordUri = $@"{Request.GetAppBaseUrl()}/api/records/{recordId}";
        return Created(recordUri, cogId);
    }

    [HttpPut("{recordId}/cogitations/{cogitationId}")]
    public async Task<IActionResult> UpdateCogitation(Guid recordId, Guid cogitationId, UpdateCogitationDto updateCogitationDto)
    {
        if (recordId != updateCogitationDto.RecordId || cogitationId != updateCogitationDto.CogitationId) return BadRequest("Not consistent request");

        await _cogitationsService.UpdateCogitationText(cogitationId, updateCogitationDto.Text);
        return NoContent();
    }

    [HttpDelete("{recordId}/cogitations/{cogitationId}")]
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
