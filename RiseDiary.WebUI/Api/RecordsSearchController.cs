using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Dto;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    [Authorize]
    public sealed class RecordsSearchController : ControllerBase
    {
        private readonly IRecordsSearchService _recordsSearchService;

        public RecordsSearchController(IRecordsSearchService recordsSearchService)
        {
            _recordsSearchService = recordsSearchService;
        }

        [HttpGet, Route("api/v1.0/records/list")]
        [ProducesResponseType(typeof(RecordsPageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            try
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

                var dto = new RecordsPageDto
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
                    }).ToList()
                };

                return Ok(dto);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499);
            }
        }

        [HttpGet, Route("api/v1.0/records/expandedlist")]
        [ProducesResponseType(typeof(RecordsDetailPageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            try
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

                var dto = new RecordsDetailPageDto
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
                            SizeKb = i.GetSizeKbString(),
                            Base64Thumbnail = i.GetBase64Thumbnail()
                        })
                        .ToArray()
                    }).ToList()
                };

                return Ok(dto);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499);
            }
        }
    }
}
