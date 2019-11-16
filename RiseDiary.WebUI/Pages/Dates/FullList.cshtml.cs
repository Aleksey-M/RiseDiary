using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Dates
{
    public class ListModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public List<DateItem> Dates;

        public ListModel(DiaryDbContext context)
        {
            _context = context;
        }

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