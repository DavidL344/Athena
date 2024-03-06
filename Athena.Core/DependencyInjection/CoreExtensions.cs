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
    private const string AppVersion
        = $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}";
    
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
        var configPaths = new Dictionary<ConfigType, string>
        {
            { ConfigType.Entries, Path.Combine(appDataDir, "entries") },
            { ConfigType.Files, Path.Combine(appDataDir, "files") },
            { ConfigType.Protocols, Path.Combine(appDataDir, "protocols") },
            { ConfigType.Plugins, Path.Combine(appDataDir, "plugins") }
        };
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
        services.AddSingleton<AppParser>();
        
        // Runner
        services.AddSingleton<RunnerOptions>();
        services.AddSingleton<AppRunner>();
        
        // User config
        Startup.Checks.CheckEntries(appDataDir, serializerOptions).GetAwaiter().GetResult();
        services.AddSingleton(GetConfig(appDataDir, serializerOptions));
    }

    internal static string GetAppDataDir()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        var userConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            OperatingSystem.IsWindows() ? "Athena" : "athena");
        var portableConfigDir = Path.Combine(assemblyDir, "user");
        
        return Directory.Exists(portableConfigDir) ? portableConfigDir : userConfigDir;
    }

    internal static Config GetConfig(string appDataDir, JsonSerializerOptions serializerOptions)
    {
        var configFile = Path.Combine(appDataDir, "config.json");
        
        if (!File.Exists(configFile))
        {
            var newConfig = new Config { Version = AppVersion };
            
            var fileContents = JsonSerializer.Serialize(newConfig, serializerOptions);
            File.WriteAllText(Path.Combine(appDataDir, "config.json"), fileContents);
            
            return newConfig;
        }
        
        var configData = File.ReadAllText(configFile);
        var config = JsonSerializer.Deserialize<Config>(configData, serializerOptions)!;
        
        if (config is null)
            throw new ApplicationException("The configuration is invalid!");
        
        if (IsConfigUpToDate(config, out var parsedConfig)) return config;
        
        var updatedConfig = JsonSerializer.Serialize(parsedConfig, serializerOptions);
        File.WriteAllText(configFile, updatedConfig);

        return parsedConfig;
    }

    internal static bool IsConfigUpToDate(Config config, out Config newConfig)
    {
        var configVersion = config.Version.Split('.');
        
        if (configVersion.Length != 3)
            throw new ApplicationException("The configuration version is invalid!");

        var appVersion = new[]
        {
            int.Parse(ThisAssembly.Git.SemVer.Major),
            int.Parse(ThisAssembly.Git.SemVer.Minor),
            int.Parse(ThisAssembly.Git.SemVer.Patch)
        };
        
        // Check the configuration version - if it's older than the current version,
        // update it with any new properties it might have
        for (var i = 0; i < configVersion.Length; i++)
        {
            if (!int.TryParse(configVersion[i], out var result))
                throw new ApplicationException("The configuration version is invalid!");

            if (result < appVersion[i])
            {
                newConfig = config with { Version = AppVersion };
                return false;
            }
            
            if (result > appVersion[i])
                throw new ApplicationException("The configuration version is newer than the application version!");
        }
        newConfig = config;
        return true;
    }
}
