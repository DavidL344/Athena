using Athena.Desktop.System;
using Microsoft.Extensions.DependencyInjection;

namespace Athena.Desktop.DependencyInjection;

public static class DesktopExtensions
{
    public static void RegisterDesktopIntegration(this IServiceCollection services)
    {
        if (!OperatingSystem.IsWindows())
        {
            services.AddSingleton<IDesktopIntegration, System.LinuxIntegration>();
            return;
        }
        services.AddSingleton<IDesktopIntegration, System.WindowsIntegration>();
    }
}
