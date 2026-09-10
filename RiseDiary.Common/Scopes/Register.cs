using Microsoft.Extensions.DependencyInjection;

namespace RiseDiary.Common.Scopes;

public static class Register
{
    public static IServiceCollection RegisterScopesAndThemesValidators(this IServiceCollection services)
    {
        services.AddSingleton<ScopeDtoValidator>();
        services.AddSingleton<ThemeDtoValidator>();

        return services;
    }
}
