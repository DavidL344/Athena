using Athena.Core.Internal.Helpers;

namespace Athena.Core.Configuration;

public class ConfigPaths
{
    public string Root { get; }
    public string ConfigFile { get; }
    public Dictionary<ConfigType, string> Subdirectories { get; }
    
    public ConfigPaths(string appDataDir)
    {
        Root = appDataDir;
        ConfigFile = Path.Combine(Root, "config.json");
        Subdirectories = new Dictionary<ConfigType, string>
        {
            { ConfigType.AppEntries, Path.Combine(Root, "entries") },
            { ConfigType.FileExtensions, Path.Combine(Root, "files") },
            { ConfigType.Protocols, Path.Combine(Root, "protocols") }
        };
    }
    
    public ConfigPaths() : this(ConfigHelper.GetAppDataDir()) { }
    
    public string GetEntryPath(string entryName, ConfigType configType)
    {
        if (string.IsNullOrWhiteSpace(entryName))
            throw new ArgumentException("The entry name is null or empty!", nameof(entryName));
        
        var parsedEntryName = GetParsedEntryName(entryName, configType);
        
        return Path.Combine(Subdirectories[configType], $"{parsedEntryName}.json");
    }

    public string GetParsedEntryName(string entryName, ConfigType configType)
        => configType switch
        {
            ConfigType.AppEntries => entryName,
            ConfigType.FileExtensions => entryName.StartsWith('.') ? entryName.Substring(1) : entryName,
            ConfigType.Protocols => entryName.Replace(":", "").Replace("/", ""),
            _ => throw new ArgumentOutOfRangeException(nameof(configType), configType, null)
        };
}
