using Athena.Core.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Athena.Core.DependencyInjection;

public static class CoreExtensions
{
    public static void AddAthenaCore(this IServiceCollection services)
    {
        services.AddSingleton<Parser>();
        services.AddSingleton<Runner>();

        services.AddSingleton(Vars.Config);
        services.AddSingleton(Vars.JsonSerializerOptions);
    }
}
