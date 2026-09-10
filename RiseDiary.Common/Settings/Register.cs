using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RiseDiary.Common.Settings.Client;
using RiseDiary.Common.Settings.Validators;

namespace RiseDiary.Common.Settings;

public static class Register
{
    public static IServiceCollection RegisterCommonSettingsValidators(this IServiceCollection services)
    {
        services.AddSingleton<AbstractValidator<ImagesSettingsDto>, ImagesSettingsValidator>();
        services.AddSingleton<AbstractValidator<ImportantDaysSettingsDto>, ImportantDaysSettingsValidator>();
        services.AddSingleton<AbstractValidator<PagesSizesSettingsDto>, PagesSizesSettingsValidator>();
        services.AddSingleton<ISettingsDtoValidator, SettingsDtoValidator>();

        return services;
    }
}
