using System.Reflection;
using System.Text.Json;
using Athena.Desktop.Configuration;
using Microsoft.Extensions.Logging;

namespace Athena.Desktop.Helpers;

public class ConfigHelper
{
    private const string AppVersion
        = $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}";
    
    public static string GetAppDataDir()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        var userConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            OperatingSystem.IsWindows() ? "Athena" : "athena");
        var portableConfigDir = Path.Combine(assemblyDir, "user");
        
        return Directory.Exists(portableConfigDir) ? portableConfigDir : userConfigDir;
    }
    
    public static Config GetConfig(ConfigPaths configPaths, JsonSerializerOptions serializerOptions)
    {
        var configFile = configPaths.ConfigFile;
        
        if (!File.Exists(configFile))
        {
            var newConfig = new Config();
            
            var fileContents = JsonSerializer.Serialize(newConfig, serializerOptions);
            File.WriteAllText(configFile, fileContents);
            
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
    
    public static bool IsConfigUpToDate(Config config, out Config newConfig)
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
                newConfig = config with
                {
                    Version = AppVersion
                };
                return false;
            }

            if (result > appVersion[i])
            {
                var mismatchMessage = $"The configuration version ({config.Version}) is newer than the application version ({AppVersion})!";
                
                if (!config.BackwardsCompatible)
                    throw new ApplicationException(mismatchMessage);
                
                using var factory = LoggerFactory.Create(builder => builder.AddConsole());
                var logger = factory.CreateLogger<ConfigHelper>();
                logger.LogWarning("{MismatchMessage}", mismatchMessage);
            }
        }
        newConfig = config;
        return true;
    }
}
