using RiseDiary.Common.Core;
using RiseDiary.WebAPI.Settings;
using RiseDiary.WebAPI.Settings.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Stubs;

internal class AppSettingsServiceStub : ISettingsService
{
    public Task<(string? value, DateTime? modifiedDate)> GetAppSetting(SettingsKey key) => key switch
    {
        _ => throw new Exception($"The stub does not know what value to return for '{key}'")
    };

    public Task<int?> GetAppSettingInt(SettingsKey key) => key switch
    {
        SettingsKey.ImageQuality => Task.FromResult((int?)75),
        SettingsKey.ThumbnailSize => Task.FromResult((int?)150),
        _ => throw new Exception($"The stub does not know what value to return for '{key}'")
    };

    public Task<Result<T>> GetSetting<T>(CancellationToken token = default) where T : ISettings
    {
        throw new NotImplementedException();
    }

    public Task<Result> UpdateSetting<T>(T setting) where T : ISettings
    {
        throw new NotImplementedException();
    }
}
