using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athena.Core.Internal;
using Athena.Core.Parser;
using Athena.Desktop.Configuration;
using Athena.Desktop.DependencyInjection;
using Athena.Desktop.Helpers;
using Athena.Desktop.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Extensions.DependencyInjection;

public static class CoreExtensions
{
    public static void AddAthenaCore(this IServiceCollection services, bool addExceptionHandler = true)
    {
        services.AddAthenaCore(ConfigHelper.GetAppDataDir(), addExceptionHandler);
    }
    
    public static void AddAthenaCore(this IServiceCollection services, string appDataDir, bool addExceptionHandler = true)
    {
        // Logging
        if (services.All(x => x.ServiceType != typeof(ILogger<>)))
            services.AddLogging(x => x.AddConsole());
        
        // Exception handling
        if (addExceptionHandler)
            AppDomain.CurrentDomain.UnhandledException += HandleException;
        
        // Config paths
        var configPaths = new ConfigPaths(appDataDir);
        services.AddSingleton(configPaths);
        
        // JSON options
        var serializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = false,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        services.AddSingleton(serializerOptions);
        
        // Parsers
        services.AddSingleton<PathParser>();
        services.AddSingleton<OpenerParser>();
        services.AddSingleton<AppEntryParser>();
        
        // Runner
        services.AddSingleton<RunnerOptions>();
        services.AddSingleton<AppRunner>();
        
        // User config
        Startup.CheckEntries(configPaths, serializerOptions).GetAwaiter().GetResult();
        var config = ConfigHelper.GetConfig(configPaths, serializerOptions);
        services.AddSingleton(config);
        
        // Desktop integration
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
            services.RegisterDesktopIntegration();
        
        // Debug logging
        if (config.Debug)
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.Debug));
        return;
        
        void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = services.BuildServiceProvider()
                .GetRequiredService<ILogger<ExceptionHandler>>();
            var exceptionObject = (Exception)e.ExceptionObject;
            
            // Throws an exception for easier debugging 
            if (Debugger.IsAttached)
                throw exceptionObject;
            
            logger.LogCritical("{Error}", exceptionObject.Message);
            
            Environment.Exit(1);
        }
    }
    
    private abstract class ExceptionHandler;
}
