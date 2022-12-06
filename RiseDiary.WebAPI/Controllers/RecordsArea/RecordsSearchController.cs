using Microsoft.AspNetCore.Mvc;
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

    public RecordsSearchController(
        IRecordsSearchService recordsSearchService,
        IRecordsSearchTextService recordsTextSearchService)
    {
        _recordsSearchService = recordsSearchService;
        _recordsTextSearchService = recordsTextSearchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecordsList(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? name,
        [FromQuery] bool? combinedThemes,
        [FromQuery] Guid[]? themeId,
        [FromQuery] int? pageSize,
        [FromQuery] int? pageNo,
        [FromQuery] bool? extended,
        CancellationToken cancellationToken)
    {
        var filters = new RecordsFilter
        {
            CombineThemes = combinedThemes ?? false,
            PageSize = pageSize ?? default,
            PageNo = pageNo ?? default,
            FromDate = from,
            ToDate = to,
            FilterName = name
        };

        if (themeId != null && themeId.Length > 0)
        {
            filters.AddThemeId(themeId);
        }

        var records = await _recordsSearchService.GetRecordsList(filters, cancellationToken);
        int allCount = await _recordsSearchService.GetRecordsCount(filters, cancellationToken);

        var pagesInfo = PagesInfo.GetPagesInfo(allCount, filters.PageNo, filters.PageSize);

        return ReturnResult(pagesInfo, records, extended);
    }


    [HttpGet("search")]
    public async Task<IActionResult> GetRecordsList(
        [FromQuery] string? searchText,
        [FromQuery] int? pageSize,
        [FromQuery] int? pageNo,
        [FromQuery] bool? extended,
        CancellationToken cancellationToken)
    {
        var filters = new RecordsTextFilter
        {
            SearchText = searchText,
            PageNo = pageNo ?? default,
            PageSize = pageSize ?? default
        };

        var records = await _recordsTextSearchService.GetRecordsList(filters, cancellationToken);
        int allCount = await _recordsTextSearchService.GetRecordsCount(filters.SearchText, cancellationToken);

        var pagesInfo = PagesInfo.GetPagesInfo(allCount, filters.PageNo, filters.PageSize);

        return ReturnResult(pagesInfo, records, extended);
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
