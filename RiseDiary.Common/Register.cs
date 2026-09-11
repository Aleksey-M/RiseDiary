using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RiseDiary.Common.Scopes;
using RiseDiary.Common.Settings;
using RiseDiary.Common.Settings.Validators;
using System.Text.Json;

namespace RiseDiary.Common;

public static class Register
{
    public static IServiceCollection RegisterJsonOptions(this IServiceCollection services)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        services.AddSingleton(options);

        return services;
    }

    public static IServiceCollection RegisterDtoValidators(this IServiceCollection services)
    {
        services.AddSingleton<AbstractValidator<ImagesSettingsDto>, ImagesSettingsValidator>();
        services.AddSingleton<AbstractValidator<ImportantDaysSettingsDto>, ImportantDaysSettingsValidator>();
        services.AddSingleton<AbstractValidator<PagesSizesSettingsDto>, PagesSizesSettingsValidator>();
        services.AddSingleton<ISettingsDtoValidator, SettingsDtoValidator>();

        services.AddSingleton<ScopeDtoValidator>();
        services.AddSingleton<ThemeDtoValidator>();

        return services;
    }
}
