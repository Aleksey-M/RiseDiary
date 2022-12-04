using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RiseDiary.Data;
using RiseDiary.Shared;
using RiseDiary.Shared.Settings;

namespace RiseDiary.Model.Services;

internal sealed class AppSettingsService : IAppSettingsService
{
    private readonly DiaryDbContext _context;

    public AppSettingsService(DiaryDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<(string? value, DateTime? modifiedDate)> GetAppSetting(AppSettingsKey key)
    {
        if (key == AppSettingsKey.Unknown) throw new ArgumentException("Unknown settings key");

        var keyStr = key.ToString();
        var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == keyStr).ConfigureAwait(false);
        return (setting?.Value, setting?.ModifiedDate);
    }

    public async Task<int?> GetAppSettingInt(AppSettingsKey key)
    {
        var (str, _) = await GetAppSetting(key).ConfigureAwait(false);
        return int.TryParse(str, NumberStyles.None, CultureInfo.CurrentCulture.NumberFormat, out int result) ? result : (int?)null;
    }

    private static int ValueOf(List<AppSetting> settings, AppSettingsKey key)
    {
        var value = settings?.FirstOrDefault(x => x.Key == key.ToString())?.Value;
        return int.TryParse(value, out var intValue) ? intValue : 0;
    }

    public async Task<ImagesSettings> GetImagesSettings(CancellationToken token)
    {
        var settings = await _context.AppSettings
            .AsNoTracking()
            .Where(x => ImagesSettings.SettingsKeys.Contains(x.Key))
            .ToListAsync(token);

        return new ImagesSettings
        {
            ThumbnailSize = ValueOf(settings, AppSettingsKey.ThumbnailSize),
            ImageQuality = ValueOf(settings, AppSettingsKey.ImageQuality)
        };
    }

    public async Task<ImportantDaysSettings> GetImportantDaysSettings(CancellationToken token)
    {
        var settings = await _context.AppSettings
            .AsNoTracking()
            .Where(x => ImportantDaysSettings.SettingsKeys.Contains(x.Key))
            .ToListAsync(token);

        var scopes = await _context.Scopes
            .AsNoTracking()
            .OrderBy(x => x.ScopeName)
            .Select(x => new KeyValuePair<Guid, string>(x.Id, x.ScopeName))
            .ToListAsync(token);

        var scopeIdString = settings.FirstOrDefault(x => x.Key == AppSettingsKey.ImportantDaysScopeId.ToString())?.Value;

        return new ImportantDaysSettings
        {
            ImportantDaysScopeId = Guid.TryParse(scopeIdString, out var id) ? id : null,
            ScopesSelectList = scopes,
            ImportantDaysDisplayRange = ValueOf(settings, AppSettingsKey.ImportantDaysDisplayRange)
        };
    }

    public async Task<PagesSizesSettings> GetPagesSizesSettings(CancellationToken token)
    {
        var settings = await _context.AppSettings
            .AsNoTracking()
            .Where(x => PagesSizesSettings.SettingsKeys.Contains(x.Key))
            .ToListAsync(token);

        return new PagesSizesSettings
        {
            AvailableImagesPageSize = ValueOf(settings, AppSettingsKey.AvailableImagesPageSize),
            ImagesPageSize = ValueOf(settings, AppSettingsKey.ImagesPageSize),
            RecordsPageSize = ValueOf(settings, AppSettingsKey.RecordsPageSize)
        };
    }

    public async Task UpdateAppSetting(AppSettingsKey key, string value)
    {
        var message = SettingsValidator.Validate(key, value);
        if (message != "") throw new ArgumentException(message);

        var keyStr = key.ToString();
        var appSetting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == keyStr).ConfigureAwait(false);

        if (appSetting == null)
        {
            await _context.AppSettings.AddAsync(new AppSetting
            {
                Key = keyStr,
                Value = value,
                ModifiedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        else
        {
            if (appSetting.Value != value)
            {
                appSetting.Value = value;
                appSetting.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }

    public Task UpdateImagesSettings(ImagesSettings imagesSettings) =>
        UpdateSettingsRange(ImagesSettings.SettingsKeys.ToList(), imagesSettings.PropertiesValues.ToList());

    public Task UpdateImportantDaysSettings(ImportantDaysSettings importantDaysSettings) =>
        UpdateSettingsRange(ImportantDaysSettings.SettingsKeys.ToList(), importantDaysSettings.PropertiesValues.ToList());

    public Task UpdatePagesSizesSettings(PagesSizesSettings pagesSizesSettings) =>
        UpdateSettingsRange(PagesSizesSettings.SettingsKeys.ToList(), pagesSizesSettings.PropertiesValues.ToList());

    private void UpdateSettings(List<AppSetting> mutableList, string key, string value)
    {
        var setting = mutableList.FirstOrDefault(x => x.Key == key);

        if (setting == null)
        {
            setting = new AppSetting
            {
                Key = key,
                ModifiedDate = DateTime.UtcNow,
                Value = value
            };

            _context.Attach(setting);
            mutableList.Add(setting);
        }
        else
        {
            if (setting.Value != value)
            {
                setting.Value = value;
                setting.ModifiedDate = DateTime.UtcNow;
            }
        }
    }

    private async Task UpdateSettingsRange(List<string> keys, List<string> values)
    {
        var settings = await _context.AppSettings
            .Where(x => keys.Contains(x.Key))
            .ToListAsync();

        var kvPairs = keys.Zip(values);

        foreach (var (k, v) in kvPairs)
        {
            UpdateSettings(settings, k, v);
        }

        await _context.SaveChangesAsync();
    }
}
