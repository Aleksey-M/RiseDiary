using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using System;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IRecordsService _recordsService;
        private readonly IAppSettingsService _settingsSvc;
        public IndexModel(IRecordsService recordsService, IAppSettingsService settingsSvc)
        {
            _recordsService = recordsService;
            _settingsSvc = settingsSvc;
        }

        public DiaryRecord? StartPageRecord { get; private set; }

        public async Task OnGetAsync()
        {
            var (sId, _) = await _settingsSvc.GetAppSetting(AppSettingsKey.StartPageRecordId);
            if (Guid.TryParse(sId, out var id))
            {
                StartPageRecord = await _recordsService.FetchRecordById(id);
            }
        }
    }
}
