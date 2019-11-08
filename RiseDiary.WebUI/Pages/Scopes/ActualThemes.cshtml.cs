using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages.Scopes
{
    public class ActualThemesModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ActualThemesModel(DiaryDbContext context)
        {
            _context = context;
        }
        public List<DiaryScope> AllScopes { get; set; }
        public async Task OnGetAsync()
        {
            AllScopes = await _context.GetAllScopes();
        }

        public async Task OnPostSetNotActualAsync(Guid themeId)
        {
            await _context.ChangeThemeActuality(themeId, false);
            AllScopes = await _context.GetAllScopes();
        }
        public async Task OnPostSetActualAsync(Guid themeId)
        {
            await _context.ChangeThemeActuality(themeId, true);
            AllScopes = await _context.GetAllScopes();
        }
    }
}