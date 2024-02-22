using Athena.CoreOld.Internal;
using Microsoft.Extensions.Hosting;

namespace Athena.CoreOld.DependencyInjection;

public static class StartupExtensions
{
    public static void InitAthena(this IHostEnvironment _)
    {
        StartupChecks.RunAsync().GetAwaiter().GetResult();
    }
}
