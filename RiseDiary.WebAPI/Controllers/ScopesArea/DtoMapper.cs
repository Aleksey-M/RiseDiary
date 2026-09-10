using RiseDiary.Common.Scopes;
using RiseDiary.WebAPI.Scopes.Model;

namespace RiseDiary.WebAPI.Controllers.ScopesArea;

internal static class DtoMapper
{
    public static ThemeDto ToDto(this ThemeEntity? theme) => theme == null
        ? throw new ArgumentNullException(nameof(theme))
        : new()
        {
            ThemeId = theme.Id,
            ScopeId = theme.ScopeId,
            ThemeName = theme.ThemeName,
            ThemeDescription = theme.Description,
            Actual = theme.Actual
        };

    public static ScopeDto ToDto(this ScopeEntity scope) => new()
    {
        ScopeId = scope.Id,
        ScopeName = scope.ScopeName,
        ScopeDescription = scope.Description,
        Themes = scope.Themes.Select(t => t.ToDto()).ToList()
    };

}
