using Microsoft.AspNetCore.Mvc;
using RiseDiary.Common.Core;
using RiseDiary.Common.Records;
using RiseDiary.WebAPI.Application.Search.Abstraction;
using RiseDiary.WebAPI.Application.Settings.Abstraction;
using RiseDiary.WebAPI.Application.Settings.Model;
using RiseDiary.WebAPI.Controllers.RecordsArea;

namespace RiseDiary.WebAPI.Controllers
{
    [ApiController]
    public class StartPageController : ControllerBase
    {
        private readonly IRecordsSearchService _recordService;

        private readonly ISettingsService _appSettingsService;

        public StartPageController(ISettingsService appSettingsService, IRecordsSearchService recordService)
        {
            _recordService = recordService;
            _appSettingsService = appSettingsService;
        }

        [HttpGet("api/bookmarks")]
        public async Task<ActionResult<Result<RecordDto[]>>> GetBookmarksRecord(CancellationToken token)
        {
            var recordIds = (await _appSettingsService.GetSetting<BookmarksSettings>(token)).Data?.Records ?? [];

            if (recordIds.Length > 0)
            {
                var recs = await _recordService.GetRecordsByIds(recordIds, token);
                return Ok(Result<RecordDto[]>.FormResult(recs, d => d.Select(x => x.ToDto()).ToArray()));
            }

            return Ok(Result<RecordDto[]>.Success([]));
        }
    }
}
