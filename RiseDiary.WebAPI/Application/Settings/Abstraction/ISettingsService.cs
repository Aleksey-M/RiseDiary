using RiseDiary.Common.Core;
using RiseDiary.WebAPI.Application.Settings.Model;

namespace RiseDiary.WebAPI.Application.Settings.Abstraction;

public interface ISettingsService
{
    Task<Result<T>> GetSetting<T>(CancellationToken token = default) where T : ISettings;
    Task<Result> UpdateSetting<T>(T setting) where T : ISettings;
}