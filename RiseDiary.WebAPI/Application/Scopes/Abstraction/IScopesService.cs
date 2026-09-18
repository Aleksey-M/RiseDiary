using RiseDiary.Common.Core;
using RiseDiary.WebAPI.Application.Scopes.Model;
using RiseDiary.WebAPI.Application.Scopes.Services.Commands;

namespace RiseDiary.WebAPI.Application.Scopes.Abstraction;

public interface IScopesService
{
    Task<Result<ScopeEntity[]>> GetScopes(CancellationToken cancellationToken = default);
    Task<Result<ApplyOperationResult>> Apply(IScopeOperation operation);
    Task<Result> DeleteScope(Guid scopeId);
    Task<Result> DeleteTheme(Guid themeId);
}

public record ApplyOperationResult(Guid? NewEntityId);