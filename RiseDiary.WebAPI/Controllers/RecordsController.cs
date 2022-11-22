﻿using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Dto;
using RiseDiary.Shared.Images;
using RiseDiary.Shared.Records;
using RiseDiary.Shared.Scopes;

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
    public async Task<IActionResult> CreateRecord(CreateRecordDto createDto)
    {
        var recId = await _recordService.AddRecord(createDto.Date, createDto.RecordName, createDto.RecordText);
        if (createDto.ThemesIds.Length > 0) await _recordsThemesService.AddRecordTheme(recId, createDto.ThemesIds);
        var newRecordUri = $@"{await _appSettingsService.GetHostAndPort()}/api/records/{recId}";
        return Created(newRecordUri, recId);
    }

    [HttpGet("{recordId}")]
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

    [HttpDelete("{recordId}")]
    public async Task<IActionResult> DeleteRecord(Guid recordId)
    {
        await _recordService.DeleteRecord(recordId);
        return NoContent();
    }

    [HttpPut("{recordId}")]
    public async Task<IActionResult> UpdateRecord(Guid recordId, UpdateRecordDto updateRecordDto)
    {
        if (recordId != updateRecordDto.Id) return BadRequest("Not consistent request");

        await _recordService.UpdateRecord(recordId, updateRecordDto.NewDate, updateRecordDto.NewName, updateRecordDto.NewText);
        return NoContent();
    }

    [HttpPost("{recordId}/cogitations")]
    public async Task<IActionResult> AddCogitation(Guid recordId, CreateCogitationDto createCogitationDto)
    {
        if (recordId != createCogitationDto.RecordId) return BadRequest("Not consistent request");

        var cogId = await _cogitationsService.AddCogitation(recordId, createCogitationDto.Text);
        var recordUri = $@"{await _appSettingsService.GetHostAndPort()}/api/v1.0/records/{recordId}";
        return Created(recordUri, cogId);
    }

    [HttpPut("{recordId}/cogitations/{cogitationId}")]
    public async Task<IActionResult> UpdateCogitation(Guid recordId, Guid cogitationId, UpdateCogitationDto updateCogitationDto)
    {
        if (recordId != updateCogitationDto.RecordId || cogitationId != updateCogitationDto.CogitationId) return BadRequest("Not consistent request");

        await _cogitationsService.UpdateCogitationText(cogitationId, updateCogitationDto.NewText);
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

    [HttpGet("startpage")]
    public async Task<ActionResult<StartPageRecordDto>> GetStartPageRecord(CancellationToken token)
    {
        var (recordId, _) = await _appSettingsService.GetAppSetting(AppSettingsKey.StartPageRecordId);

        if (recordId != null && Guid.TryParse(recordId, out var id))
        {
            var rec = await _recordService.FetchRecordById(id, token);
            return Ok(new StartPageRecordDto
            {
                RecordId = id,
                RecordMdText = rec.Text
            });
        }

        return Ok(new StartPageRecordDto());
    }
}
