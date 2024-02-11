using Athena.Core.Internal;
using Microsoft.Extensions.Hosting;

namespace Athena.Core.DependencyInjection;

public static class StartupExtensions
{
    public static void InitAthena(this IHostEnvironment _)
    {
        StartupChecks.RunAsync().GetAwaiter().GetResult();
    }
}
