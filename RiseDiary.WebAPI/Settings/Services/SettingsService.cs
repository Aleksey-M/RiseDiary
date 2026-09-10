using RiseDiary.Common.Core;
using RiseDiary.WebAPI.Settings.Model;

namespace RiseDiary.WebAPI.Settings.Services;


public class SettingsService : ISettingsService
{
    private readonly IServiceProvider _provider;
    protected readonly ILogger<SettingsService>? _logger;

    public SettingsService(IServiceProvider provider, ILogger<SettingsService>? logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public virtual async Task<Result<T>> GetSetting<T>(CancellationToken token = default) where T : ISettings
    {
        try
        {
            var storage = _provider.GetRequiredService<ISettingsStorage<T>>();
            var setting = await storage.Read(token);
            return Result<T>.Success(setting);
        }
        catch (Exception exc)
        {
            _logger?.LogError(exc, "При получении настройки {settingKey} произошла критическая ошибка", typeof(T).Name);
            return Result<T>.Failure(exc.Message);
        }
    }

    public virtual async Task<Result> UpdateSetting<T>(T setting) where T : ISettings
    {
        try
        {
            var storage = _provider.GetRequiredService<ISettingsStorage<T>>();
            await storage.Write(setting);
            _logger?.LogInformation("Обновлена настройка {settingKey}: {settingValue}", typeof(T).Name, setting);

            return Result.Success();
        }
        catch (Exception exc)
        {
            _logger?.LogError(exc,
                "При сохранении настройки {settingKey} произошла критическая ошибка: {errorMessage}",
                typeof(T).Name, exc.Message);
            return Result.Failure(exc.Message);
        }
    }
}
