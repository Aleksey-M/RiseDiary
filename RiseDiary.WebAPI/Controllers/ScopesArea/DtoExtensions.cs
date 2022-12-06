using RiseDiary.Model;
using RiseDiary.Shared.Scopes;

namespace RiseDiary.WebAPI.Controllers.ScopesArea;

internal static class DtoExtensions
{
    public static ThemeDto ToDto(this DiaryTheme? theme) => theme == null
        ? throw new ArgumentNullException(nameof(theme))
        : new()
        {
            ThemeId = theme.Id,
            ScopeId = theme.ScopeId,
            ThemeName = theme.ThemeName,
            ThemeDescription = theme.Description,
            Actual = theme.Actual
        };

    public static ScopeDto ToDto(this DiaryScope scope) => new()
    {
        ScopeId = scope.Id,
        ScopeName = scope.ScopeName,
        ScopeDescription = scope.Description,
        Themes = scope.Themes.Select(t => t.ToDto()).ToList()
    };

}
