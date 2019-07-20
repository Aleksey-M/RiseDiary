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
        private Guid? _datesScopeId;
        private int _daysDisplayRange;
        private readonly DiaryDbContext _context;

        public List<DateItem> Dates;
        public bool IsScopeSelected => _datesScopeId != null && _datesScopeId.Value != Guid.Empty;

        public IndexModel(DiaryDbContext context)
        {
            _context = context;            
        }

        private async Task UpdateViewModel()
        {            
            var stringId = await _context.GetAppSetting(AppSettingsKeys.DatesScopeId);
            _datesScopeId = Guid.Parse(stringId);
           if(IsScopeSelected)
            {
                _daysDisplayRange = (int)(await _context.GetAppSettingInt(AppSettingsKeys.DatesDisplayRange));
                var datesRange = new DatesRange(DateTime.Now, _daysDisplayRange);

                Dates = await _context.FetchDateItems(_datesScopeId.Value, datesRange);
                
                var weekdays = datesRange.AllRangeDates
                    .Where(di => !Dates.Any(d => d.TransferredDate == di.TransferredDate));

                Dates.AddRange(weekdays);
                Dates = Dates.OrderByDescending(d => d.TransferredDate).ToList();
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