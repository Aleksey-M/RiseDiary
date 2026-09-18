using RiseDiary.WebAPI.Application.Scopes.Abstraction;
using RiseDiary.WebAPI.Application.Scopes.Services;
using RiseDiary.WebAPI.Application.Settings.Abstraction;
using RiseDiary.WebAPI.Application.Settings.Model;
using RiseDiary.WebAPI.Application.Settings.Services;
using RiseDiary.WebAPI.Application.Settings.Storage;

namespace RiseDiary.WebAPI.Application;

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

    public static IServiceCollection RegisterScopesServices(this IServiceCollection collection)
    {
        collection.AddScoped<ScopesService>();
        collection.AddScoped<IScopesService, CachedScopesService>();

        return collection;
    }

    public static IServiceCollection RegisterApplicationServices(this IServiceCollection collection) => collection
        .RegisterSettingsServices()
        .RegisterScopesServices();
}
