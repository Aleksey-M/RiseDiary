using RiseDiary.Common.Core;

namespace RiseDiary.Common.Scopes.Client;

public interface IScopesClient
{
    Task<Result<Guid>> CreateScope(ScopeDto dto, CancellationToken ct = default);
    Task<Result<Guid>> CreateTheme(Guid scopeId, ThemeDto dto, CancellationToken ct = default);

    Task<Result<List<ScopeDto>>> GetScopes(bool? actual = null, CancellationToken ct = default);

    Task<Result> UpdateScope(Guid id, ScopeDto dto, CancellationToken ct = default);
    Task<Result> DeleteScope(Guid id, CancellationToken ct = default);

    Task<Result> UpdateTheme(Guid scopeId, Guid themeId, ThemeDto dto, CancellationToken ct = default);
    Task<Result> UpdateThemeActuality(Guid scopeId, Guid themeId, ThemeDto dto, CancellationToken ct = default);
    Task<Result> DeleteTheme(Guid scopeId, Guid themeId, CancellationToken ct = default);
}
