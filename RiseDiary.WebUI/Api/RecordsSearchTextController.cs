using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Api
{
    [ApiController]
    public class RecordsSearchTextController : ControllerBase
    {
        private readonly IRecordsSearchTextService _recordsSearchService;

        public RecordsSearchTextController(IRecordsSearchTextService recordsSearchService)
        {
            _recordsSearchService = recordsSearchService;
        }

        [HttpGet, Route("api/v1.0/records/textsearch/list")]
        [ProducesResponseType(typeof(RecordsPageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecordsPageDto>> GetRecordsList([FromQuery] string? searchText, [FromQuery] int? pageSize, [FromQuery] int? pageNo)
        {
            try
            {
                pageSize ??= 20;
                pageSize = pageSize > 100 ? 100 : pageSize;
                pageNo ??= 0;

                var filters = new RecordsTextFilter
                {
                    SearchText = searchText,
                    PageNo = pageNo.Value,
                    PageSize = pageSize.Value
                };

                var records = await _recordsSearchService.GetRecordsList(filters);
                int allCount = await _recordsSearchService.GetRecordsCount(filters.SearchText);

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
        }

        [HttpGet, Route("api/v1.0/records/textsearch/expandedlist")]
        [ProducesResponseType(typeof(RecordsDetailPageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecordsDetailPageDto>> GetRecordsListExpanded([FromQuery] string? searchText, [FromQuery] int? pageSize, [FromQuery] int? pageNo)
        {
            try
            {
                pageSize ??= 20;
                pageSize = pageSize > 100 ? 100 : pageSize;
                pageNo ??= 0;

                pageSize ??= 20;
                pageSize = pageSize > 100 ? 100 : pageSize;
                pageNo ??= 0;

                var filters = new RecordsTextFilter
                {
                    SearchText = searchText,
                    PageNo = pageNo.Value,
                    PageSize = pageSize.Value
                };

                var records = await _recordsSearchService.GetRecordsList(filters);
                int allCount = await _recordsSearchService.GetRecordsCount(filters.SearchText);

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
        }
    }
}
