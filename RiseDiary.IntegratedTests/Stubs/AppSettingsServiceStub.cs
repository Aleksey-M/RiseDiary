using System;
using System.Threading;
using System.Threading.Tasks;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Settings;

namespace RiseDiary.IntegratedTests.Stubs;

internal class AppSettingsServiceStub : IAppSettingsService
{
    public Task<(string? value, DateTime? modifiedDate)> GetAppSetting(AppSettingsKey key) => key switch
    {
        _ => throw new Exception($"The stub does not know what value to return for '{key}'")
    };

    public Task<int?> GetAppSettingInt(AppSettingsKey key) => key switch
    {
        AppSettingsKey.ImageQuality => Task.FromResult((int?)75),
        AppSettingsKey.ThumbnailSize => Task.FromResult((int?)150),
        _ => throw new Exception($"The stub does not know what value to return for '{key}'")
    };

    public Task<ImagesSettings> GetImagesSettings(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<ImportantDaysSettings> GetImportantDaysSettings(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<PagesSizesSettings> GetPagesSizesSettings(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAppSetting(AppSettingsKey key, string value) => throw new NotImplementedException();

    public Task UpdateImagesSettings(ImagesSettings imagesSettings)
    {
        throw new NotImplementedException();
    }

    public Task UpdateImportantDaysSettings(ImportantDaysSettings importantDaysSettings)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePagesSizesSettings(PagesSizesSettings pagesSizesSettings)
    {
        throw new NotImplementedException();
    }
}
