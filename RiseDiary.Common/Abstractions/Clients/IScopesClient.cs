using RiseDiary.Common.Core;
using RiseDiary.Common.Scopes;

namespace RiseDiary.Common.Abstractions.Clients;

public interface IScopesClient
{
    Task<Result<Guid>> CreateScope(ScopeDto dto);
    Task<Result<Guid>> CreateTheme(ThemeDto dto);

    Task<Result<List<ScopeDto>>> GetScopes(CancellationToken ct = default);

    Task<Result> UpdateScope(ScopeDto dto);
    Task<Result> DeleteScope(Guid id);

    Task<Result> UpdateTheme(ThemeDto dto);
    Task<Result> DeleteTheme(Guid scopeId, Guid themeId);
}
