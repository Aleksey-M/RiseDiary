using Microsoft.AspNetCore.Mvc;
using RiseDiary.Common;
using RiseDiary.Common.Core;
using RiseDiary.Common.Records;
using RiseDiary.Common.Search;
using RiseDiary.WebAPI.Application.Records.Model;
using RiseDiary.WebAPI.Application.Search.Abstraction;
using RiseDiary.WebAPI.Application.Settings.Abstraction;
using RiseDiary.WebAPI.Application.Settings.Model;

namespace RiseDiary.WebAPI.Controllers.RecordsArea;

[ApiController]
[Route("api/records")]
public sealed class RecordsSearchController : ControllerBase
{
    private readonly IRecordsSearchService _recordsSearchService;
    private readonly IRecordsSearchTextService _recordsTextSearchService;
    private readonly ISettingsService _appSettingsService;

    public RecordsSearchController(
        IRecordsSearchService recordsSearchService,
        IRecordsSearchTextService recordsTextSearchService,
        ISettingsService appSettingsService)
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
        int pageSize = (await _appSettingsService.GetSetting<PagesSizesSettings>(cancellationToken)).Data?.RecordsPageSize ?? 50;
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
            foreach (var themeId in themes.Split(","))
            {
                if (Guid.TryParse(themeId, out var id))
                {
                    filters.AddThemeId(id);
                }
            }
        }

        var result = await _recordsSearchService.GetRecords(filters, cancellationToken);
        var pagesInfo = PagesInfo.GetPagesInfo(result?.Data.count ?? 0, filters.PageNo + 1, filters.PageSize);

        return ReturnResult(pagesInfo, result!, expanded);
    }


    [HttpGet("search")]
    public async Task<IActionResult> GetRecordsList(
        [FromQuery] string? searchText,
        [FromQuery] int? page,
        [FromQuery] bool? expanded,
        CancellationToken cancellationToken)
    {
        int pageSize = (await _appSettingsService.GetSetting<PagesSizesSettings>(cancellationToken)).Data?.RecordsPageSize ?? 50;
        var filters = new RecordsTextFilter
        {
            SearchText = searchText,
            PageNo = 1,
            PageSize = pageSize
        };

        var records = await _recordsTextSearchService.GetRecordsList(filters, cancellationToken);
        var pagesInfo = PagesInfo.GetPagesInfo(records.Data.count, page ?? 1, pageSize);
        return ReturnResult(pagesInfo, records, expanded);
    }

    [HttpGet("this-day/{date}")]
    public async Task<IActionResult> GetThisDayRecords(CancellationToken cancellationToken, string? date)
    {
        var today = !string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var today_selected)
            ? today_selected
            : DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await _recordsSearchService.GetThisDayRecords(month: today.Month, day: today.Day, cancellationToken);

        if (!result.Succeeded || (result?.Data?.Length ?? 0) == 0)
        {
            return NoContent();
        }

        return Ok(Result<RecordListItemDto[]>.FormResult(result!, entities => entities.Select(x => x.ToDto()).ToArray()));
    }

    private IActionResult ReturnResult(PagesInfo pagesInfo, Result<(RecordEntity[] records, int count)> result, bool? extended) => (extended ?? false)
            ? Ok(Result<RecordsPageDto<RecordDto>>.FormResult(result, records => new RecordsPageDto<RecordDto>
            {
                PagesInfo = pagesInfo,
                Records = records.records
                    .Select(x => x.ToDto())
                    .ToList()
            }))
            : Ok(Result<RecordsPageDto<RecordListItemDto>>.FormResult(result, records => new RecordsPageDto<RecordListItemDto>
            {
                PagesInfo = pagesInfo,
                Records = records.records
                .Select(x => x.ToListDto())
                .ToList()
            }));
}
