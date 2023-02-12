using System;
using System.Threading;
using System.Threading.Tasks;
using RiseDiary.Model;
using RiseDiary.Shared;
using RiseDiary.Shared.Settings;

namespace RiseDiary.IntegratedTests.Stubs;

internal class AppSettingsForDatesServiceStub : IAppSettingsService
{
    private readonly int _range;
    public AppSettingsForDatesServiceStub(int range)
    {
        _range = range;
    }

    public Task<(string? value, DateTime? modifiedDate)> GetAppSetting(AppSettingsKey key) => key switch
    {
        AppSettingsKey.ImportantDaysScopeId => Task.FromResult<(string?, DateTime?)>(("00000000-0000-0000-0000-000000000001", null)),
        _ => throw new Exception($"The stub does not know what value to return for '{key}'")
    };

    public Task<int?> GetAppSettingInt(AppSettingsKey key) => key switch
    {
        AppSettingsKey.ImportantDaysDisplayRange => Task.FromResult((int?)_range),
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
