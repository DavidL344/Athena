using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athena.Core.Model;
using Athena.Core.Parser;
using Athena.Core.Parser.Shared;
using Athena.Core.Runner;
using Athena.Core.Runner.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Athena.Core.DependencyInjection;

public static class CoreExtensions
{
    public static void AddAthenaCore(this IServiceCollection services)
    {
        services.AddAthenaCore(GetAppDataDir());
    }
    
    public static void AddAthenaCore(this IServiceCollection services, string appDataDir)
    {
        // Logging
        if (services.All(x => x.ServiceType != typeof(ILogger<>)))
            services.AddLogging();
        
        // Config paths
        services.AddSingleton<Dictionary<ConfigType, string>>(_ =>
            new Dictionary<ConfigType, string>
            {
                { ConfigType.Entries, Path.Combine(appDataDir, "entries") },
                { ConfigType.Files, Path.Combine(appDataDir, "files") },
                { ConfigType.Protocols, Path.Combine(appDataDir, "protocols") }
            });
        
        // JSON options
        services.AddSingleton<JsonSerializerOptions>(_ =>
            new JsonSerializerOptions
            {
                AllowTrailingCommas = false,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
        
        // Parsers
        services.AddSingleton<PathParser>();
        services.AddSingleton<OpenerParser>();
        services.AddSingleton<AppParser>();
        
        // Runner
        services.AddSingleton<RunnerOptions>();
        services.AddSingleton<AppRunner>();
        
        // User config
        services.AddSingleton<Config>(x =>
        {
            var serializerOptions = x.GetRequiredService<JsonSerializerOptions>();
            
            Startup.Checks.CheckConfiguration(appDataDir, serializerOptions).GetAwaiter().GetResult();
            
            return GetConfig(appDataDir, serializerOptions);
        });
    }

    private static string GetAppDataDir()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        var userConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "athena");
        var portableConfigDir = Path.Combine(assemblyDir, "user");
        
        return Directory.Exists(portableConfigDir) ? portableConfigDir : userConfigDir;
    }

    private static Config GetConfig(string appDataDir, JsonSerializerOptions serializerOptions)
    {
        var appConfigPath = Path.Combine(appDataDir, "config.json");
        
        var configData = File.ReadAllText(appConfigPath);
        var config = JsonSerializer.Deserialize<Config>(configData, serializerOptions)!;
            
        if (config is null)
            throw new ApplicationException("The configuration is invalid!");
            
        return config;
    }
}
