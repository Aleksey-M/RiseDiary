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

        private async Task UpdateViewModel()
        {
            var stringId = await _context.GetAppSetting(AppSettingsKeys.DatesScopeId);
            Guid.TryParse(stringId, out var _datesScopeId);

            if (IsScopeSelected)
            {
                _daysDisplayRange = (int)(await _context.GetAppSettingInt(AppSettingsKeys.DatesDisplayRange));
                var datesRange = new DatesRange(DateTime.Now, _daysDisplayRange);

                Dates = await _context.FetchDateItems(_datesScopeId, datesRange);

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