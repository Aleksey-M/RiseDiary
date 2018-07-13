using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages.Scopes
{
    public class ActualThemesModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ActualThemesModel(DiaryDbContext context)
        {
            _context = context;
        }
        public List<DiaryThemeJoined> Themes { get; set; }        
        public async Task OnGetAsync()
        {
            Themes = await _context.FetchThemesWithScopes();
        }
        
        public async Task OnPostSetNotActualAsync(int themeId)
        {
            if(themeId != 0)
            {
                await _context.ChangeThemeActuality(themeId, false);
            }
            Themes = await _context.FetchThemesWithScopes();
        }
        public async Task OnPostSetActualAsync(int themeId)
        {
            if (themeId != 0)
            {
                await _context.ChangeThemeActuality(themeId, true);
            }
            Themes = await _context.FetchThemesWithScopes();
        }
    }
}