using Athena.Core.Parser.Shared;

namespace Athena.Core.Configuration;

public class ConfigPaths
{
    public string Root { get; }
    public string File { get; }
    public Dictionary<ConfigType, string> Subdirectories { get; }

    public ConfigPaths(string appDataDir)
    {
        Root = appDataDir;
        File = Path.Combine(Root, "config.json");
        Subdirectories = new Dictionary<ConfigType, string>
        {
            { ConfigType.Entries, Path.Combine(Root, "entries") },
            { ConfigType.Files, Path.Combine(Root, "files") },
            { ConfigType.Protocols, Path.Combine(Root, "protocols") }
        };
    }
}
