using Microsoft.AspNetCore.Mvc;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Records;

namespace RiseDiary.WebAPI.Controllers
{
    [Route("api/startpage")]
    [ApiController]
    public class StartPageController : ControllerBase
    {
        private readonly IRecordsService _recordService;

        private readonly IAppSettingsService _appSettingsService;

        public StartPageController(IAppSettingsService appSettingsService, IRecordsService recordService)
        {
            _recordService = recordService;
            _appSettingsService = appSettingsService;
        }

        [HttpGet]
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
}
