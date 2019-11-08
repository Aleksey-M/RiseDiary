using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RiseDiary.WebUI.Pages
{
    public class ScopePageModel : PageModel
    {
        private readonly DiaryDbContext _context;
        public ScopePageModel(DiaryDbContext context)
        {
            _context = context;
        }

        public List<DiaryScope> AllScopes { get; private set; }

        public async Task OnGetAsync()
        {
            await UpdatePageState();
        }

        private async Task UpdatePageState()
        {
            AllScopes = await _context.GetAllScopes();      
        }

        public async Task OnPostAddScopeAsync(string newScopeName)
        {
            if (!string.IsNullOrWhiteSpace(newScopeName))
            {
                await _context.AddScope(newScopeName);
            }
            else
            {
                ModelState.AddModelError("Scope Name", "�������� ������� �� �������");
            }
            await UpdatePageState();
        }

        public async Task OnPostUpdateScopeAsync(Guid scopeId, string scopeName)
        {
            if (!string.IsNullOrWhiteSpace(scopeName))
            {
                await _context.UpdateScope(new DiaryScope { Id = scopeId, ScopeName = scopeName });
            }
            else
            {
                ModelState.AddModelError("Scope Name", "�������� ������� �� �������");
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteScopeAsync(Guid scopeId)
        {
            if (await _context.CanDeleteScope(scopeId))
            {
                await _context.DeleteScope(scopeId);
            }
            else
            {
                ModelState.AddModelError("Scope", "������� �� ����� ���� �������. ���� ��������� ����");
            }
            await UpdatePageState();
        }

        public async Task OnPostAddThemeAsync(Guid scopeId, string newThemeName)
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
                    ModelState.AddModelError("Theme Name", "�������� ����� ���� �� �������");
                }
            }
            else
            {
                ModelState.AddModelError("Theme Name", $"������� ��������� � Id={scopeId} �� �������");
            }
            await UpdatePageState();
        }

        public async Task OnPostDeleteThemeAsync(Guid themeId)
        {
            await _context.DeleteTheme(themeId);
            await UpdatePageState();
        }

        public async Task OnPostUpdateThemeAsync(Guid themeId, string newThemeName)
        {
            if (!string.IsNullOrWhiteSpace(newThemeName))
            {
                await _context.UpdateTheme(new DiaryTheme { Id = themeId, ThemeName = newThemeName });
            }
            else
            {
                ModelState.AddModelError("Theme Name", "�������� ���� �� �������");
            }
            await UpdatePageState();
        }
    }
}