using RiseDiary.WebAPI.Application.Settings.Model;

namespace RiseDiary.WebAPI.Application.Settings.Abstraction;

public interface ISettingsStorage<T> where T : ISettings
{
    Task<T> Read(CancellationToken cancellationToken);
    Task Write(T settings);
}
