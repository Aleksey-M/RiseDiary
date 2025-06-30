using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using RiseDiary.Data;
using RiseDiary.Shared;
using RiseDiary.Shared.Settings;
using RiseDiary.WebAPI.Caching;

namespace RiseDiary.Model.Services;

public sealed class AppSettingsService : IAppSettingsService
{
    private readonly DiaryDbContext _context;
    private readonly ILogger<AppSettingsService> _logger;
    private readonly HybridCache _cache;

    public AppSettingsService(DiaryDbContext context, ILogger<AppSettingsService> logger, HybridCache cache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
        _cache = cache;
    }

    public async Task<(string? value, DateTime? modifiedDate)> GetAppSetting(AppSettingsKey key)
    {
        if (key == AppSettingsKey.Unknown)
            throw new ArgumentException("Unknown settings key");

        var setting = await _cache.GetOrCreateAsync(
            CacheTags.AppSettingKey(key),
            async ct =>
            {
                _logger.LogInformation("Чтение настройки {settingsKey} из БД", key);

                return await _context.AppSettings
                    .FirstOrDefaultAsync(s => s.Key == key.ToString(), cancellationToken: ct);
            },
            tags: [key.ToString()]);

        return (setting?.Value, setting?.ModifiedDate);
    }

    public async Task<int?> GetAppSettingInt(AppSettingsKey key)
    {
        var (str, _) = await GetAppSetting(key);
        return int.TryParse(str, NumberStyles.None, CultureInfo.CurrentCulture.NumberFormat, out int result) ? result : (int?)null;
    }

    public async Task<ImagesSettings> GetImagesSettings() =>
        new ImagesSettings
        {
            ThumbnailSize = await GetAppSettingInt(AppSettingsKey.ThumbnailSize) ?? 0,
            ImageQuality = await GetAppSettingInt(AppSettingsKey.ImageQuality) ?? 0
        };

    public async Task<ImportantDaysSettings> GetImportantDaysSettings(CancellationToken token)
    {
        var (ids, _) = await GetAppSetting(AppSettingsKey.ImportantDaysScopeId);

        var scopes = await _cache.GetOrCreateAsync(
            CacheTags.ScopesNames,
            async ct =>
            {
                _logger.LogInformation("Чтение списка 'Увлечений' из БД");

                return await _context.Scopes
                    .OrderBy(x => x.ScopeName)
                    .Select(x => new KeyValuePair<Guid, string>(x.Id, x.ScopeName))
                    .ToListAsync(ct);
            },
            tags: [CacheTags.ScopesNames],
            cancellationToken: token);


        return new ImportantDaysSettings
        {
            ImportantDaysScopeId = Guid.TryParse(ids, out var id) ? id : null,
            ScopesSelectList = scopes,
            ImportantDaysDisplayRange = await GetAppSettingInt(AppSettingsKey.ImportantDaysDisplayRange) ?? 0
        };
    }

    public async Task<PagesSizesSettings> GetPagesSizesSettings() =>
        new PagesSizesSettings
        {
            AvailableImagesPageSize = await GetAppSettingInt(AppSettingsKey.AvailableImagesPageSize) ?? 0,
            ImagesPageSize = await GetAppSettingInt(AppSettingsKey.ImagesPageSize) ?? 0,
            RecordsPageSize = await GetAppSettingInt(AppSettingsKey.RecordsPageSize) ?? 0
        };


    public async Task UpdateAppSetting(AppSettingsKey key, string value)
    {
        var message = SettingsValidator.Validate(key, value);
        if (message != "") throw new ArgumentException(message);

        var keyStr = key.ToString();
        var appSetting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == keyStr);

        if (appSetting == null)
        {
            _context.AppSettings.Add(new AppSetting
            {
                Key = keyStr,
                Value = value,
                ModifiedDate = DateTime.UtcNow
            });
        }
        else
        {
            appSetting.ModifiedDate = appSetting.Value != value ? DateTime.UtcNow : appSetting.ModifiedDate;
            appSetting.Value = value;
        }

        await _context.SaveChangesAsync();

        var tag = CacheTags.AppSettingKey(key);
        await _cache.RemoveByTagAsync(tag);
        await _cache.SetAsync(tag, value, tags: [key.ToString()]);

        _logger.LogInformation("Обновлена настройка {configKey} = {configValue}", key, value);
    }

    public async Task UpdateImagesSettings(ImagesSettings imagesSettings)
    {
        await UpdateAppSetting(AppSettingsKey.ImageQuality, imagesSettings.ImageQuality.ToString());
        await UpdateAppSetting(AppSettingsKey.ThumbnailSize, imagesSettings.ThumbnailSize.ToString());
    }

    public async Task UpdateImportantDaysSettings(ImportantDaysSettings importantDaysSettings)
    {
        await UpdateAppSetting(AppSettingsKey.ImportantDaysScopeId, importantDaysSettings.ImportantDaysScopeId?.ToString() ?? "");
        await UpdateAppSetting(AppSettingsKey.ImportantDaysDisplayRange, importantDaysSettings.ImportantDaysDisplayRange.ToString());
    }

    public async Task UpdatePagesSizesSettings(PagesSizesSettings pagesSizesSettings)
    {
        await UpdateAppSetting(AppSettingsKey.ImagesPageSize, pagesSizesSettings.ImagesPageSize.ToString());
        await UpdateAppSetting(AppSettingsKey.RecordsPageSize, pagesSizesSettings.RecordsPageSize.ToString());
        await UpdateAppSetting(AppSettingsKey.AvailableImagesPageSize, pagesSizesSettings.AvailableImagesPageSize.ToString());
    }
}
