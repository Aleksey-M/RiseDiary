using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Dates
{
    public class IndexModel : PageModel
    {
        private readonly IDatesService _datesService;
        private readonly IAppSettingsService _settingsSvc;

        public IndexModel(IDatesService datesService, IAppSettingsService settingsSvc)
        {
            _datesService = datesService;
            _settingsSvc = settingsSvc;
        }

        public DateTime Today { get; } = DateTime.Now.Date;
        public IEnumerable<DateListItem> Dates { get; private set; } = Enumerable.Empty<DateListItem>();

        public async Task<IActionResult> OnGetAsync()
        {
            var (stringId, _) = await _settingsSvc.GetAppSetting(AppSettingsKey.ImportantDaysScopeId);
            var range = await _settingsSvc.GetAppSettingInt(AppSettingsKey.ImportantDaysDisplayRange);

            if (!Guid.TryParse(stringId, out _) || range == null) return Redirect("~/Dates/Setup");

            Dates = await _datesService.GetDatesFromRange(Today, true);
            return Page();
        }

        public string GetRowStyle(DateListItem date) => (!string.IsNullOrEmpty(date.Themes), date.TransferredDate == Today) switch
        {
            (true, true) => "background-color:#fe9393",
            (true, false) => "background-color:#ebfb78",
            (false, true) => "background-color:#ffb98a",
            (false, false) => "background-color:#b0f8c4"
        };
    }
}