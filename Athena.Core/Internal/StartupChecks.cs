using System.Text.Json;
using Athena.Core.Model;
using Athena.Core.Model.Entry;
using Athena.Core.Model.Opener;

namespace Athena.Core.Internal;

public static class StartupChecks
{
    public static async Task RunAsync()
    {
        var configDirNoDirs = Directory.GetDirectories(Vars.AppDataDir).Length == 0;
        var configDirNoFiles = Directory.GetFiles(Vars.AppDataDir).Length == 0;
        
        var firstRun = !Directory.Exists(Vars.AppDataDir) || (configDirNoDirs && configDirNoFiles);
        if (firstRun) Directory.CreateDirectory(Vars.AppDataDir);
        
        var subDirs = new[] { "entries", "files", "protocols" };
        foreach (var subDir in subDirs)
        {
            var path = Path.Combine(Vars.AppDataDir, subDir);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        
        if (firstRun) await GenerateConfigSamples();
    }

    private static async Task GenerateConfigSamples()
    {
        var entries = new Dictionary<string, AppEntry>
        {
            {
                "athena.protocol", new AppEntry
                {
                    Name = "Athena",
                    Type = EntryType.Protocol,
#if DEBUG
                    Path = Path.Combine(Vars.AssemblyDir, "Athena"),
#else
                    Path = "athena",
#endif
                    Arguments = "run $URL -l",
                    RemoveProtocol = true
                }
            },
            {
                "firefox.open", new AppEntry
                {
                    Name = "Firefox (Open)",
                    Type = EntryType.All,
                    Path = "firefox",
                    Arguments = "$URL"
                }
            },
            {
                "firefox.open-private", new AppEntry
                {
                    Name = "Firefox (Private)",
                    Type = EntryType.All,
                    Path = "firefox",
                    Arguments = "firefox -private-window $URL"
                }
            },
            {
                "unzip.extract", new AppEntry
                {
                    Name = "Unzip (Extract)",
                    Type = EntryType.File,
                    Path = "unzip",
                    Arguments = "$FILE"
                }
            },
            {
                "unzip.extract-to-desktop", new AppEntry
                {
                    Name = "Unzip (Extract to Desktop)",
                    Type = EntryType.File,
                    Path = "unzip",
                    Arguments = "-d ~/Desktop $FILE"
                }
            },
            {
                "unzip.list", new AppEntry
                {
                    Name = "Unzip (List)",
                    Type = EntryType.File,
                    Path = "unzip",
                    Arguments = "-l $FILE"
                }
            }
        };
        
        var fileExtensions = new Dictionary<string, FileExtension>
        {
            {
                "zip", new FileExtension
                {
                    Name = "Zip Archive",
                    AppList =
                    [
                        "unzip.extract",
                        "unzip.extract-to-desktop",
                        "unzip.list"
                    ]
                }
            },
            {
                "7z", new FileExtension
                {
                    Name = "7-Zip Archive",
                    AppList =
                    [
                        "unzip.extract",
                        "unzip.extract-to-desktop",
                        "unzip.list"
                    ]
                }
            }
        };
        
        var protocols = new Dictionary<string, Protocol>
        {
            {
                "athena", new Protocol
                {
                    Name = "athena",
                    AppList =
                    [
                        "athena.protocol"
                    ],
                    DefaultApp = "athena.protocol"
                }
            },
            {
                "http", new Protocol
                {
                    Name = "http",
                    AppList =
                    [
                        "firefox.open",
                        "firefox.open-private"
                    ],
                    DefaultApp = "firefox.open"
                }
            },
            {
                "https", new Protocol
                {
                    Name = "https",
                    AppList =
                    [
                        "firefox.open",
                        "firefox.open-private"
                    ],
                    DefaultApp = "firefox.open"
                }
            }
        };

        foreach (var entry in entries)
        {
            var filePath = Path.Combine(Vars.ConfigPaths[ConfigType.Entries], $"{entry.Key}.json");
            var fileContents = JsonSerializer.Serialize(entry.Value, Vars.JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        }

        foreach (var fileExtension in fileExtensions)
        {
            var filePath = Path.Combine(Vars.ConfigPaths[ConfigType.Files], $"{fileExtension.Key}.json");
            var fileContents = JsonSerializer.Serialize(fileExtension.Value, Vars.JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        }
        
        foreach (var protocol in protocols)
        {
            var filePath = Path.Combine(Vars.ConfigPaths[ConfigType.Protocols], $"{protocol.Key}.json");
            var fileContents = JsonSerializer.Serialize(protocol.Value, Vars.JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        }
    }
}
