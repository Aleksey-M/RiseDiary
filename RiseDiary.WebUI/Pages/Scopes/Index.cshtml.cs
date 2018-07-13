using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;

namespace RiseDiary.WebUI.Pages
{
    public class ScopePageModel : PageModel
    {
        private readonly DiaryDbContext _context;
        private readonly ILogger<ScopePageModel> _logger;
        public ScopePageModel(DiaryDbContext context, ILogger<ScopePageModel> logger)
        {
            _context = context;
            _logger = logger;            
        }

        public Dictionary<DiaryScope, IEnumerable<DiaryTheme>> ScopesAndThemes { get; private set; }
        
        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }
               
        private async Task UpdatePageState()
        {
            ScopesAndThemes = await _context.FetchScopesWithThemes();            
        }

        public async Task OnPostAddScopeAsync(string newScopeName)
        {
            if (!string.IsNullOrWhiteSpace(newScopeName))
            {
                await _context.AddScope(newScopeName);
            }
            else
            {
                ModelState.AddModelError("Scope Name", "Название области не введено");
            }
            await UpdatePageState();
        }

        public async Task OnPostUpdateScopeAsync(int scopeId, string scopeName)
        {
            if (!string.IsNullOrWhiteSpace(scopeName))
            {
                await _context.UpdateScope(new DiaryScope { Id = scopeId, ScopeName = scopeName });
            }
            else
            {
                ModelState.AddModelError("Scope Name", "Название области не введено");
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteScopeAsync(int scopeId)
        {
            if (await _context.CanDeleteScope(scopeId))
            {
                await _context.DeleteScope(scopeId);
            }
            else
            {
                ModelState.AddModelError("Scope", "Область не может быть удалена. Есть связанные темы");
            }
            await UpdatePageState();
        }

        public async Task OnPostAddThemeAsync(int scopeId, string newThemeName)
        {
            var area = await _context.FetchScopeById(scopeId);
            if (area != null)
            {
                if (!string.IsNullOrWhiteSpace(newThemeName))
                {
                    await _context.AddTheme(scopeId, newThemeName);
                }
                else
                {
                    ModelState.AddModelError("Theme Name", "Название новой темы не введено");
                }
            }
            else
            {
                ModelState.AddModelError("Theme Name", $"Область интересов с Id={scopeId} не найдена");
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteThemeAsync(int themeId)
        {
            await _context.DeleteTheme(themeId);
            await UpdatePageState();
        }

        public async Task OnPostUpdateThemeAsync(int themeId, string newThemeName)
        {
            if (!string.IsNullOrWhiteSpace(newThemeName))
            {
                await _context.UpdateTheme(new DiaryTheme { Id = themeId, ThemeName = newThemeName });
            }
            else
            {
                ModelState.AddModelError("Theme Name", "Название темы не введено");
            }
            await UpdatePageState();
        }
    }
}