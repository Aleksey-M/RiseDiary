using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Dates
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class CalendarModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public List<DateItem> Dates;

        public CalendarModel(DiaryDbContext context)
        {
            _context = context;
        }

        private string LocalHostAndPort => Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

        public async Task<IActionResult> OnGetAsync()
        {
            var scopeId = await _context.GetAppSetting(AppSettingsKeys.DatesScopeId);
            Guid sId = default;
            if (scopeId != null && Guid.TryParse(scopeId, out sId))
            {
                Dates = await _context.FetchAllDateItems(sId, LocalHostAndPort);
                return Page();
            }
            return Redirect("~/Dates/setup");
        }
    }
}