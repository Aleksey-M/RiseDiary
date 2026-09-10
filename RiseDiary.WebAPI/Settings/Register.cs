using RiseDiary.WebAPI.Settings.Model;
using RiseDiary.WebAPI.Settings.Services;
using RiseDiary.WebAPI.Settings.Storage;

namespace RiseDiary.WebAPI.Settings;

public static class Register
{
    /// <summary>
    /// регистрация сервисов и хранилищ для настроек приложения
    /// </summary>
    public static IServiceCollection RegisterSettingsServices(this IServiceCollection collection)
    {
        collection.AddScoped<ISettingsStorage<ImportantDaysSettings>, ImportantDaysSettingsStorage>();
        collection.AddScoped<ISettingsStorage<BookmarksSettings>, BookmarksSettingsStorage>();
        collection.AddScoped<ISettingsStorage<PagesSizesSettings>, PagesSizesSettingsStorage>();
        collection.AddScoped<ISettingsStorage<ImagesSettings>, ImagesSettingsStorage>();

        collection.AddScoped<SettingsService>();
        collection.AddScoped<ISettingsService, CachedSettingsService>();

        return collection;
    }
}
