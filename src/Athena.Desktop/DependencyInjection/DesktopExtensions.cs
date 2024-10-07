using System.Runtime.Versioning;
using Athena.Desktop.System;
using Microsoft.Extensions.DependencyInjection;

namespace Athena.Desktop.DependencyInjection;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("windows")]
public static class DesktopExtensions
{
    public static void RegisterDesktopIntegration(this IServiceCollection services)
    {
        if (OperatingSystem.IsLinux())
        {
            services.AddSingleton<IDesktopIntegration, LinuxIntegration>();
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IDesktopIntegration, WindowsIntegration>();
        }
    }
}
