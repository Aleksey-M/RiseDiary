using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.Shared;

namespace RiseDiary.WebUI.Pages.Dates
{
    public class ListModel : PageModel
    {
        private readonly IDatesService _datesService;
        private readonly IAppSettingsService _settingsSvc;

        public ListModel(IDatesService datesService, IAppSettingsService settingsSvc)
        {
            _datesService = datesService;
            _settingsSvc = settingsSvc;
        }

        public IEnumerable<DateListItem> Dates { get; private set; } = Enumerable.Empty<DateListItem>();

        public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
        {
            var (stringId, _) = await _settingsSvc.GetAppSetting(AppSettingsKey.ImportantDaysScopeId);
            var range = await _settingsSvc.GetAppSettingInt(AppSettingsKey.ImportantDaysDisplayRange);

            if (!Guid.TryParse(stringId, out _) || range == null) return Redirect("~/Dates/Setup");

            try
            {
                Dates = await _datesService.GetAllDates(DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }

            return Page();
        }
    }
}