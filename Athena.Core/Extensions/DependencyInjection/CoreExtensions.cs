using System.Text.Json;
using System.Text.Json.Serialization;
using Athena.Core.Configuration;
using Athena.Core.Internal;
using Athena.Core.Internal.Helpers;
using Athena.Core.Options;
using Athena.Core.Parser;
using Athena.Core.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Extensions.DependencyInjection;

public static class CoreExtensions
{
    public static void AddAthenaCore(this IServiceCollection services)
    {
        services.AddAthenaCore(ConfigHelper.GetAppDataDir());
    }
    
    public static void AddAthenaCore(this IServiceCollection services, string appDataDir)
    {
        // Logging
        if (services.All(x => x.ServiceType != typeof(ILogger<>)))
            services.AddLogging();
        
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
        services.AddSingleton(ConfigHelper.GetConfig(configPaths, serializerOptions));
    }
}
