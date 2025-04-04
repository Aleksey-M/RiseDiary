using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Records;
using RiseDiary.WebAPI.Controllers.RecordsArea;

namespace RiseDiary.WebAPI.Controllers
{
    [ApiController]
    public class StartPageController : ControllerBase
    {
        private readonly IRecordsSearchService _recordService;

        private readonly IAppSettingsService _appSettingsService;

        public StartPageController(IAppSettingsService appSettingsService, IRecordsSearchService recordService)
        {
            _recordService = recordService;
            _appSettingsService = appSettingsService;
        }

        [HttpGet("api/bookmarks")]
        public async Task<ActionResult<List<RecordDto>>> GetBookmarksRecord(CancellationToken token)
        {
            var (recordIds, _) = await _appSettingsService.GetAppSetting(AppSettingsKey.BookmarksRecordsList);

            if (!string.IsNullOrEmpty(recordIds))
            {
                var ids = recordIds
                    .Trim()
                    .Split(';')
                    .Select(x => (isGuid: Guid.TryParse(x, out var id), id))
                    .Where(x => x.isGuid)
                    .Select(x => x.id)
                    .ToArray();

                var recs = await _recordService.GetRecordsByIds(ids, token);
                return Ok(recs.Select(x => x.ToDto()).ToList());
            }

            return Ok(new List<RecordDto>());
        }
    }
}
