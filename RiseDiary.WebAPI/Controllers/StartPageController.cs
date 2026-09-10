using Microsoft.AspNetCore.Mvc;
using RiseDiary.Common.Records;
using RiseDiary.Model;
using RiseDiary.WebAPI.Controllers.RecordsArea;
using RiseDiary.WebAPI.Settings;
using RiseDiary.WebAPI.Settings.Model;

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
        public async Task<ActionResult<List<RecordDto>>> GetBookmarksRecord(CancellationToken token)
        {
            var recordIds = (await _appSettingsService.GetSetting<BookmarksSettings>(token)).Data?.Records ?? [];

            if (recordIds.Length > 0)
            {
                var recs = await _recordService.GetRecordsByIds(recordIds, token);
                return Ok(recs.Select(x => x.ToDto()).ToList());
            }

            return Ok(new List<RecordDto>());
        }
    }
}
