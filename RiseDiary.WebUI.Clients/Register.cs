using Microsoft.Extensions.DependencyInjection;
using RiseDiary.Common.Abstractions.Clients;

namespace RiseDiary.WebUI.Clients;

public static class Register
{
    public static IServiceCollection RegisterWebClients(this IServiceCollection services)
    {
        services.AddSingleton<ISettingsClient, SettingsClient>();
        services.AddSingleton<IScopesClient, ScopesClient>();

        return services;
    }
}
