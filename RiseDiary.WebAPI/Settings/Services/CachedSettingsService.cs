using Microsoft.Extensions.Caching.Memory;
using RiseDiary.Common.Core;

namespace RiseDiary.WebAPI.Settings.Services;

public class CachedSettingsService : SettingsService
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3));

    public CachedSettingsService(IServiceProvider provider, ILogger<SettingsService> logger, IMemoryCache cache) : base(provider, logger)
    {
        _cache = cache;
    }

    public override async Task<Result<T>> GetSetting<T>(CancellationToken token = default)
    {
        var key = typeof(T).Name;
        if (_cache.TryGetValue(key, out T? value) && value != null)
        {
            return Result<T>.Success(value);
        }

        var val = await base.GetSetting<T>(token);
        if (val.Succeeded)
        {
            _cache.Set(key, val.Data, _cacheEntryOptions);
        }

        return val;
    }

    public override async Task<Result> UpdateSetting<T>(T setting)
    {
        var result = await base.UpdateSetting(setting);

        var key = typeof(T).Name;
        _cache.Remove(key);

        return result;
    }
}
