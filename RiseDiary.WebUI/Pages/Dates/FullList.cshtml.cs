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

        public async Task<IActionResult> OnGetAsync()
        {
            var scopeId = await _context.GetAppSettingInt(AppSettingsKeys.DatesScopeId);
            if(scopeId == null)
            {
                return Redirect("~/Dates/setup");
            }
            Dates = await _context.FetchAllDateItems(scopeId.Value);
            return Page();
        }
    }
}