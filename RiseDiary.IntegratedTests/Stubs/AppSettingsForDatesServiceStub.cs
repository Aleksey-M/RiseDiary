using RiseDiary.Common.Core;
using RiseDiary.WebAPI.Settings;
using RiseDiary.WebAPI.Settings.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RiseDiary.IntegratedTests.Stubs;

internal class AppSettingsForDatesServiceStub : ISettingsService
{
    private readonly int _range;
    public AppSettingsForDatesServiceStub(int range)
    {
        _range = range;
    }

    public Task<(string? value, DateTime? modifiedDate)> GetAppSetting(SettingsKey key) => key switch
    {
        SettingsKey.ImportantDaysScopeId => Task.FromResult<(string?, DateTime?)>(("00000000-0000-0000-0000-000000000001", null)),
        _ => throw new Exception($"The stub does not know what value to return for '{key}'")
    };

    public Task<int?> GetAppSettingInt(SettingsKey key) => key switch
    {
        SettingsKey.ImportantDaysDisplayRange => Task.FromResult((int?)_range),
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
