using RiseDiary.Common.Core;

namespace RiseDiary.Common.Settings.Client;

public interface ISettingsClient
{
    Task<Result<object?>> GetAsync(SettingKey key, CancellationToken ct = default);
    Task<Result> UpdateAsync(SettingKey key, object value, CancellationToken ct = default);
}