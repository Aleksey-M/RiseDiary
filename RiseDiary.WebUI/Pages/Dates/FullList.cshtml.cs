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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
    public class ListModel : PageModel
    {
        private readonly DiaryDbContext _context;        

        public ListModel(DiaryDbContext context)
        {
            _context = context;
        }

        public IEnumerable<DateItem> Dates { get; private set; } = Enumerable.Empty<DateItem>();

        private string LocalHostAndPort => Request.Scheme + @"://" + Request.Host.Host + ":" + Request.Host.Port;

        public async Task<IActionResult> OnGetAsync()
        {
            var scopeId = await _context.GetAppSetting(AppSettingsKeys.DatesScopeId);
            if(scopeId == null)
            {
                return Redirect("~/Dates/setup");
            }
            if (Guid.TryParse(scopeId, out var sId))
            {
                Dates = await _context.FetchAllDateItems(sId, LocalHostAndPort);
            }
            else
                throw new Exception($"Incorrect format of Scope ID in the AppSettings: {scopeId}");

            return Page();
        }
    }
}