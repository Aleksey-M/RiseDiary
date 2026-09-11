using RiseDiary.Common.Core;
using RiseDiary.Common.Settings;

namespace RiseDiary.Common.Abstractions.Clients;

public interface ISettingsClient
{
    Task<Result<object?>> GetAsync(SettingKey key, CancellationToken ct = default);
    Task<Result> UpdateAsync(SettingKey key, object value, CancellationToken ct = default);
}