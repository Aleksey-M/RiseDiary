using Microsoft.Extensions.DependencyInjection;
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
}
