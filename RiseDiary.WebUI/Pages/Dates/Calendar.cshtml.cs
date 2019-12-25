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
    public class CalendarModel : PageModel
    {
        private readonly DiaryDbContext _context;

        public CalendarModel(DiaryDbContext context)
        {
            _context = context;
        }

        public IEnumerable<DateItem> Dates { get; private set; } = Enumerable.Empty<DateItem>();
        private string LocalHostAndPort => Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

        public async Task<IActionResult> OnGetAsync()
        {
            var scopeId = await _context.GetAppSetting(AppSettingsKeys.DatesScopeId);
            if (scopeId != null && Guid.TryParse(scopeId, out Guid sId))
            {
                Dates = await _context.FetchAllDateItems(sId, LocalHostAndPort);
                return Page();
            }
            return Redirect("~/Dates/setup");
        }
    }
}