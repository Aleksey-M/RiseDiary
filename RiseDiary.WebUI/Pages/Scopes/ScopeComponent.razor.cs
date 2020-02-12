using Microsoft.AspNetCore.Components;
using RiseDiary.Model;
using RiseDiary.WebUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
namespace RiseDiary.WebUI.Pages
{
    public partial class ScopeComponent
    {
        [Parameter] public DiaryDbContext DbContext { get; set; } = null!;
        [Parameter] public DiaryScope Scope { get; set; } = null!;
        [Parameter] public Func<Task> UpdateParentState { get; set; } = null!;
        private List<DiaryTheme> Themes { get; set; } = new List<DiaryTheme>();

        private string? ErrorMessage { get; set; }
        private string? CreateThemeErrorMessage { get; set; }
        private string? EditThemeErrorMessage { get; set; }
        private string? NewThemeName { get; set; }
        private bool EditMode { get; set; }
        private string? ScopeNewName { get; set; }
        private Guid? EditedThemeId { get; set; }
        private string? ThemeNewName { get; set; }

        private void EditTheme(Guid themeId)
        {
            EditedThemeId = themeId;
            ThemeNewName = Scope.Themes.Single(t => t.Id == themeId).ThemeName;
            EditThemeErrorMessage = null;
        }

        private void ShowScopeNameEditField()
        {
            ScopeNewName = Scope.ScopeName;
            EditMode = true;
        }

        private async Task UpdateScopeName()
        {
            if (string.IsNullOrWhiteSpace(ScopeNewName))
            {
                ErrorMessage = "Нужно ввести новое название области интересов";
                return;
            }

            Scope.ScopeName = ScopeNewName.Trim();
            await DbContext.UpdateScope(Scope);
            EditMode = false;
            ErrorMessage = null;
        }

        private async Task UpdateThemeName()
        {
            if (EditedThemeId == null) return;

            if (string.IsNullOrWhiteSpace(ThemeNewName))
            {
                EditThemeErrorMessage = "Нужно ввести новое название темы";
                return;
            }

            if (Scope.Themes.Any(t => t.ThemeName.ToUpperInvariant() == ThemeNewName.ToUpperInvariant() && t.Id != EditedThemeId.Value))
            {
                EditThemeErrorMessage = $"Тема с названием '{ThemeNewName}' уже есть";
                return;
            }

            var theme = Scope.Themes.Single(t => t.Id == EditedThemeId.Value);
            theme.ThemeName = ThemeNewName;
            await DbContext.UpdateTheme(theme);
            ThemeNewName = null;
            EditThemeErrorMessage = null;
            EditedThemeId = null;
        }

        private async Task DeleteTheme()
        {
            if (EditedThemeId == null) return;
            await DbContext.DeleteTheme(EditedThemeId.Value);
            EditedThemeId = null;
            EditThemeErrorMessage = null;
            Scope = await DbContext.FetchScopeById(Scope.Id);
            this.StateHasChanged();
        }

        private async Task TryRemoveScope()
        {
            if (await DbContext.CanDeleteScope(Scope.Id))
            {
                await DbContext.DeleteScope(Scope.Id);
                await UpdateParentState();
            }
            else
            {
                ErrorMessage = "Область не может быть удалена. Есть связанные темы";
            }
        }

        private async Task CreateNewTheme()
        {
            if (string.IsNullOrWhiteSpace(NewThemeName))
            {
                CreateThemeErrorMessage = "Нужно ввести название новой темы";
                return;
            }

            if (Scope.Themes.Any(t => t.ThemeName.ToUpperInvariant() == NewThemeName.ToUpperInvariant()))
            {
                CreateThemeErrorMessage = $"Тема с названием '{NewThemeName}' уже есть";
                return;
            }

            await DbContext.AddTheme(Scope.Id, NewThemeName);
            NewThemeName = null;
            CreateThemeErrorMessage = null;
            Scope = await DbContext.FetchScopeById(Scope.Id);
        }

        public async Task ToggleActuality(Guid themeId)
        {
            var theme = Scope.Themes.Single(t => t.Id == themeId);
            await DbContext.ChangeThemeActuality(theme.Id, !theme.Actual);
            Scope = await DbContext.FetchScopeById(Scope.Id);
        }
    }
}
