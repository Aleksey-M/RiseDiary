using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Dto;
using RiseDiary.Shared.Images;
using RiseDiary.Shared.Scopes;

namespace RiseDiary.Api;

[ApiController]
[Route("api/records")]
public sealed class RecordsSearchController : ControllerBase
{
    private readonly IRecordsSearchService _recordsSearchService;

    public RecordsSearchController(IRecordsSearchService recordsSearchService)
    {
        _recordsSearchService = recordsSearchService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<RecordsPageDto>> GetRecordsList(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? name,
        [FromQuery] bool? combinedThemes,
        [FromQuery] Guid[]? themeId,
        [FromQuery] int? pageSize,
        [FromQuery] int? pageNo,
        CancellationToken cancellationToken)
    {

        pageSize ??= 20;
        pageSize = pageSize > 100 ? 100 : pageSize;
        pageNo ??= 0;

        var filters = new RecordsFilter
        {
            CombineThemes = combinedThemes ?? false,
            PageSize = pageSize.Value,
            PageNo = pageNo.Value,
            FromDate = from.HasValue ? DateOnly.FromDateTime(from.Value) : null,
            ToDate = to.HasValue ? DateOnly.FromDateTime(to.Value) : null,
            FilterName = name
        };

        if (themeId != null && themeId.Length > 0)
        {
            filters.AddThemeId(themeId);
        }

        var records = await _recordsSearchService.GetRecordsList(filters, cancellationToken);
        int allCount = await _recordsSearchService.GetRecordsCount(filters, cancellationToken);

        var pagesInfo = PagesInfo.GetPagesInfo(allCount, pageNo.Value, pageSize.Value);

        return new RecordsPageDto
        {
            PagesInfo = pagesInfo,
            Records = records.Select(r => new RecordListItemDto
            {
                Date = r.Date,
                CreatedDate = r.CreateDate,
                ModifiedDate = r.ModifyDate,
                DisplayedName = r.GetRecordNameDisplay(),
                DisplayedText = r.GetRecordTextShort(),
                RecordId = r.Id
            })
            .ToList()
        };
    }

    [HttpGet("expandedlist")]
    public async Task<ActionResult<RecordsDetailPageDto>> GetRecordsListExpanded(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? name,
        [FromQuery] bool? combinedThemes,
        [FromQuery] Guid[]? themeId,
        [FromQuery] int? pageSize,
        [FromQuery] int? pageNo,
        CancellationToken cancellationToken)
    {

        pageSize ??= 20;
        pageSize = pageSize > 100 ? 100 : pageSize;
        pageNo ??= 0;

        var filters = new RecordsFilter
        {
            CombineThemes = combinedThemes ?? false,
            PageSize = pageSize.Value,
            PageNo = pageNo.Value,
            FromDate = from.HasValue ? DateOnly.FromDateTime(from.Value) : null,
            ToDate = to.HasValue ? DateOnly.FromDateTime(to.Value) : null,
            FilterName = name
        };

        if (themeId != null && themeId.Length > 0)
        {
            filters.AddThemeId(themeId);
        }

        var records = await _recordsSearchService.GetRecordsList(filters, cancellationToken);
        int allCount = await _recordsSearchService.GetRecordsCount(filters, cancellationToken);

        var pagesInfo = PagesInfo.GetPagesInfo(allCount, pageNo.Value, pageSize.Value);

        return new RecordsDetailPageDto
        {
            PagesInfo = pagesInfo,
            Records = records.Select(record => new RecordDto
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
                        SizeKb = i.SizeByte.ToFileSizeString(),
                        Base64Thumbnail = i.GetBase64Thumbnail()
                    })
                    .ToArray()
            })
            .ToList()
        };
    }
}
