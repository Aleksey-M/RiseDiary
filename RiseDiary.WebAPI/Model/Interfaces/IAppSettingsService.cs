using RiseDiary.Shared;
using RiseDiary.Shared.Settings;

namespace RiseDiary.Model;

public interface IAppSettingsService
{
    Task<(string? value, DateTime? modifiedDate)> GetAppSetting(AppSettingsKey key);

    Task<int?> GetAppSettingInt(AppSettingsKey key);

    Task UpdateAppSetting(AppSettingsKey key, string value);

    Task<ImagesSettings> GetImagesSettings();

    Task UpdateImagesSettings(ImagesSettings imagesSettings);

    Task<ImportantDaysSettings> GetImportantDaysSettings(CancellationToken token);

    Task UpdateImportantDaysSettings(ImportantDaysSettings importantDaysSettings);

    Task<PagesSizesSettings> GetPagesSizesSettings();

    Task UpdatePagesSizesSettings(PagesSizesSettings pagesSizesSettings);
}