using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.WebUI.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Dates
{
    public class CalendarModel : PageModel
    {
        private readonly IDatesService _datesService;
        private readonly IAppSettingsService _settingsSvc;

        public CalendarModel(IDatesService datesService, IAppSettingsService settingsSvc)
        {
            _datesService = datesService;
            _settingsSvc = settingsSvc;
        }

        public IEnumerable<DateListItem> Dates { get; private set; } = Enumerable.Empty<DateListItem>();
        public DateTime Today { get; } = DateTime.Now.Date;

        public async Task<IActionResult> OnGetAsync()
        {
            var (stringId, _) = await _settingsSvc.GetAppSetting(AppSettingsKey.ImportantDaysScopeId);
            var range = await _settingsSvc.GetAppSettingInt(AppSettingsKey.ImportantDaysDisplayRange);

            if (!Guid.TryParse(stringId, out _) || range == null) return Redirect("~/Dates/Setup");

            Dates = await _datesService.GetAllDates(DateTime.Now.Date);
            return Page();
        }        
    }
}