using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Dates
{
    public class IndexModel : PageModel
    {
        private int? _datesScopeId;
        private int _daysDisplayRange;
        private readonly DiaryDbContext _context;

        public List<DateItem> Dates;
        public bool IsScopeSelected => _datesScopeId >= 0;

        public IndexModel(DiaryDbContext context)
        {
            _context = context;            
        }

        private async Task UpdateViewModel()
        {            
            _datesScopeId = await _context.GetAppSettingInt(AppSettingsKeys.DatesScopeId);
           if(IsScopeSelected)
            {
                _daysDisplayRange = (int)(await _context.GetAppSettingInt(AppSettingsKeys.DatesDisplayRange));
                Dates = await _context.FetchDateItems((int)_datesScopeId, DateTime.Now, _daysDisplayRange);

                var from = DateTime.Now.AddDays(-_daysDisplayRange);
                var to = DateTime.Now.AddDays(_daysDisplayRange);

                var weekdays = Enumerable
                    .Range(0, _daysDisplayRange * 2 + 1)
                    .Select(i => new DateItem(from.AddDays(i)))
                    .Where(di => !Dates.Any(d => d.ThisYearDate == di.ThisYearDate));

                Dates.AddRange(weekdays);
                Dates = Dates.OrderByDescending(d => d.ThisYearDate).ToList();
            }          
        }

        public async Task<IActionResult>  OnGetAsync()
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