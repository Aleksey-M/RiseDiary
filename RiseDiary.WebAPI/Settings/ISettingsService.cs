using RiseDiary.Common.Core;
using RiseDiary.WebAPI.Settings.Model;

namespace RiseDiary.WebAPI.Settings;

public interface ISettingsService
{
    Task<Result<T>> GetSetting<T>(CancellationToken token = default) where T : ISettings;
    Task<Result> UpdateSetting<T>(T setting) where T : ISettings;
}