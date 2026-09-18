using Microsoft.Extensions.Caching.Memory;
using RiseDiary.Common.Core;
using RiseDiary.Data;
using RiseDiary.WebAPI.Application.Scopes.Abstraction;
using RiseDiary.WebAPI.Application.Scopes.Model;
using RiseDiary.WebAPI.Application.Scopes.Services.Commands;

namespace RiseDiary.WebAPI.Application.Scopes.Services;

public class CachedScopesService : ScopesService
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
    private const string CACHE_KEY = "Scopes";

    public CachedScopesService(DiaryDbContext context, ILogger<ScopesService> logger, IMemoryCache cache) : base(context, logger)
    {
        _cache = cache;
    }

    public override async Task<Result<ApplyOperationResult>> Apply(IScopeOperation operation)
    {
        var result = await base.Apply(operation);
        _cache.Remove(CACHE_KEY);
        return result;
    }

    public override async Task<Result> DeleteScope(Guid scopeId)
    {
        var result = await base.DeleteScope(scopeId);
        _cache.Remove(CACHE_KEY);
        return result;
    }

    public override async Task<Result> DeleteTheme(Guid themeId)
    {
        var result = await base.DeleteTheme(themeId);
        _cache.Remove(CACHE_KEY);
        return result;
    }

    public override async Task<Result<ScopeEntity[]>> GetScopes(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CACHE_KEY, out ScopeEntity[]? value) && value != null)
        {
            return Result<ScopeEntity[]>.Success(value);
        }

        var val = await base.GetScopes(cancellationToken);
        if (val.Succeeded)
        {
            _cache.Set(CACHE_KEY, val.Data, _cacheEntryOptions);
        }

        return val;
    }
}
