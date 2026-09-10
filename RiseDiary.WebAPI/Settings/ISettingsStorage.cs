using RiseDiary.WebAPI.Settings.Model;

namespace RiseDiary.WebAPI.Settings;

public interface ISettingsStorage<T> where T : ISettings
{
    Task<T> Read(CancellationToken cancellationToken);
    Task Write(T settings);
}
