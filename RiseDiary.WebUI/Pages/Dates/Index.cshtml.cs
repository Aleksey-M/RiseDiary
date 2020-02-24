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
        private Guid _datesScopeId = Guid.Empty;
        private int _daysDisplayRange;
        private readonly DiaryDbContext _context;
        
        public bool IsScopeSelected => _datesScopeId != Guid.Empty;

        public IndexModel(DiaryDbContext context)
        {
            _context = context;
        }

        public IEnumerable<DateItem> Dates { get; private set; } = Enumerable.Empty<DateItem>();

        private string LocalHostAndPort => Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

        private async Task UpdateViewModel()
        {
            var stringId = await _context.GetAppSetting(AppSettingsKeys.DatesScopeId);
            if(!Guid.TryParse(stringId, out _datesScopeId)) 
            {
                _datesScopeId = Guid.Empty;
            }

            if (IsScopeSelected)
            {
                _daysDisplayRange = (await _context.GetAppSettingInt(AppSettingsKeys.DatesDisplayRange)) ?? 7;
                var datesRange = new DatesRange(DateTime.Now, _daysDisplayRange);

                var dates = await _context.FetchDateItems(_datesScopeId, datesRange, LocalHostAndPort);

                var weekdays = datesRange.AllRangeDates
                    .Where(di => !dates.Any(d => d.TransferredDate == di.TransferredDate));

                dates.AddRange(weekdays);
                Dates = dates.OrderByDescending(d => d.TransferredDate).ToList();
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