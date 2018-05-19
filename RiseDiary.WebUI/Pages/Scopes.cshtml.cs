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

        private IEnumerable<DiaryScope> _scopes = new List<DiaryScope>();
        public IEnumerable<DiaryScope> Scopes { get => _scopes; }
        private IEnumerable<DiaryThemeJoined> _recThemes = new List<DiaryThemeJoined>();
        public IEnumerable<DiaryThemeJoined> Themes { get => _recThemes; }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }
               
        private async Task UpdatePageState()
        {
            _scopes = await _context.FetchAllScopes();
            _recThemes = await _context.FetchThemesWithScopes();
        }

        public async Task OnPostAddScopeAsync(string newScopeName)
        {
            if (!string.IsNullOrWhiteSpace(newScopeName))
            {
                await _context.AddScope(newScopeName);
            }
            else
            {
                ModelState.AddModelError("Theme Name", "Название темы не введено");
            }
            await UpdatePageState();
        }

        public async Task OnPostUpdateScopeNameAsync(int scopeId, string scopeName)
        {
            if (!string.IsNullOrWhiteSpace(scopeName))
            {
                await _context.UpdateScope(new DiaryScope { Id = scopeId, ScopeName = scopeName });
            }
            else
            {
                ModelState.AddModelError("Theme Name", "Название темы не введено");
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
                ModelState.AddModelError("Area", "Область не может быть удалена. Есть связанные темы");
            }
            await UpdatePageState();
        }

        public async Task OnPostAddNewThemeAsync(int scopeIdForTheme, string newThemeName)
        {
            var area = await _context.FetchScopeById(scopeIdForTheme);
            if (area != null)
            {
                if (!string.IsNullOrWhiteSpace(newThemeName))
                {
                    await _context.AddTheme(scopeIdForTheme, newThemeName);
                }
                else
                {
                    ModelState.AddModelError("Theme Name", "Название новой темы не введено");
                }
            }
            else
            {
                ModelState.AddModelError("Theme Name", $"Область интересов с Id={scopeIdForTheme} не найдена");
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteThemeAsync(int themeId)
        {
            await _context.DeleteTheme(themeId);
            await UpdatePageState();
        }

        public async Task OnPostUpdateThemeNameAsync(int themeId, string themeName)
        {
            if (!string.IsNullOrWhiteSpace(themeName))
            {
                await _context.UpdateTheme(new DiaryTheme { Id = themeId, ThemeName = themeName });
            }
            else
            {
                ModelState.AddModelError("Theme Name", "Название темы не введено");
            }
            await UpdatePageState();
        }
    }
}