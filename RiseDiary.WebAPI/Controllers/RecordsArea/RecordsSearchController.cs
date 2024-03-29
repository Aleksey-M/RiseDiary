﻿using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Records;

namespace RiseDiary.WebAPI.Controllers.RecordsArea;

[ApiController]
[Route("api/records")]
public sealed class RecordsSearchController : ControllerBase
{
    private readonly IRecordsSearchService _recordsSearchService;

    private readonly IRecordsSearchTextService _recordsTextSearchService;

    private readonly IAppSettingsService _appSettingsService;

    public RecordsSearchController(
        IRecordsSearchService recordsSearchService,
        IRecordsSearchTextService recordsTextSearchService,
        IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService;
        _recordsSearchService = recordsSearchService;
        _recordsTextSearchService = recordsTextSearchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecordsList(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? name,
        [FromQuery] bool? combinedThemes,
        [FromQuery] string? themes,
        [FromQuery] int? page,
        [FromQuery] bool? expanded,
        CancellationToken cancellationToken)
    {
        int pageSize = await _appSettingsService.GetAppSettingInt(AppSettingsKey.RecordsPageSize) ?? 50;

        int actualPage = page.HasValue && page.Value > 0 ? page.Value - 1 : 0;

        var filters = new RecordsFilter
        {
            CombineThemes = combinedThemes ?? false,
            PageSize = pageSize,
            PageNo = actualPage,
            FromDate = from,
            ToDate = to,
            FilterName = name
        };

        if (themes != null)
        {
            foreach(var themeId in themes.Split(","))
            {
                if(Guid.TryParse(themeId, out var id))
                {
                    filters.AddThemeId(id);
                }
            }
        }

        int allCount = await _recordsSearchService.GetRecordsCount(filters, cancellationToken);
        var records = await _recordsSearchService.GetRecordsList(filters, cancellationToken);        

        var pagesInfo = PagesInfo.GetPagesInfo(allCount, filters.PageNo + 1, filters.PageSize);

        return ReturnResult(pagesInfo, records, expanded);
    }


    [HttpGet("search")]
    public async Task<IActionResult> GetRecordsList(
        [FromQuery] string? searchText,
        [FromQuery] int? page,
        [FromQuery] bool? expanded,
        CancellationToken cancellationToken)
    {
        int pageSize = await _appSettingsService.GetAppSettingInt(AppSettingsKey.RecordsPageSize) ?? 50;
        int allCount = await _recordsTextSearchService.GetRecordsCount(searchText, cancellationToken);
        var pagesInfo = PagesInfo.GetPagesInfo(allCount, page ?? 1, pageSize);

        var filters = new RecordsTextFilter
        {
            SearchText = searchText,
            PageNo = pagesInfo.CurrentPage - 1,
            PageSize = pagesInfo.PageSize
        };

        var records = await _recordsTextSearchService.GetRecordsList(filters, cancellationToken);

        return ReturnResult(pagesInfo, records, expanded);
    }

    [HttpGet("this-day")]
    public async Task<IActionResult> GetThisDayRecords(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var records = await _recordsSearchService.GetThisDayRecords(month: today.Month, day: today.Day, cancellationToken);

        return records.Count > 0
            ? Ok(records.Select(x => x.ToDto()).ToList())
            : NoContent();
    }

    private IActionResult ReturnResult(PagesInfo pagesInfo, List<DiaryRecord> records, bool? extended) => (extended ?? false)
            ? Ok(new RecordsPageDto<RecordDto>
            {
                PagesInfo = pagesInfo,
                Records = records
                    .Select(x => x.ToDto())
                    .ToList()
            })
            : Ok(new RecordsPageDto<RecordListItemDto>
            {
                PagesInfo = pagesInfo,
                Records = records
                .Select(x => x.ToListDto())
                .ToList()
            });
}
