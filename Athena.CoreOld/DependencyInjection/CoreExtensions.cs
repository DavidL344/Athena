using Athena.CoreOld.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Athena.CoreOld.DependencyInjection;

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
