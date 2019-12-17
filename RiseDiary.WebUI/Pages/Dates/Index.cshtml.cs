using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Dates
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class IndexModel : PageModel
    {
        private Guid _datesScopeId = default;
        private int _daysDisplayRange;
        private readonly DiaryDbContext _context;

        public List<DateItem> Dates;
        public bool IsScopeSelected => _datesScopeId == default;

        public IndexModel(DiaryDbContext context)
        {
            _context = context;
        }

        private string LocalHostAndPort => Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

        private async Task UpdateViewModel()
        {
            var stringId = await _context.GetAppSetting(AppSettingsKeys.DatesScopeId);
            Guid.TryParse(stringId, out var _datesScopeId);

            if (IsScopeSelected)
            {
                _daysDisplayRange = (await _context.GetAppSettingInt(AppSettingsKeys.DatesDisplayRange)) ?? 7;
                var datesRange = new DatesRange(DateTime.Now, _daysDisplayRange);

                Dates = await _context.FetchDateItems(_datesScopeId, datesRange, LocalHostAndPort);

                var weekdays = datesRange.AllRangeDates
                    .Where(di => !Dates.Any(d => d.TransferredDate == di.TransferredDate));

                Dates.AddRange(weekdays);
                Dates = Dates.OrderByDescending(d => d.TransferredDate).ToList();
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await UpdateViewModel();
            if (!IsScopeSelected)
            {
                return Redirect("~/Dates/Setup");
            }

            return Page();
        }
    }
}