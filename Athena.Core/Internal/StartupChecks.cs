using System.Text.Json;
using Athena.Core.Model;

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
                "unzip.extract", new AppEntry
                {
                    Name = "Unzip (Extract)",
                    Type = AppEntry.EntryType.File,
                    Path = "unzip",
                    Arguments = "$FILE"
                }
            },
            {
                "unzip.extract-to-desktop", new AppEntry
                {
                    Name = "Unzip (Extract to Desktop)",
                    Type = AppEntry.EntryType.File,
                    Path = "unzip",
                    Arguments = "-d ~/Desktop $FILE"
                }
            },
            {
                "unzip.list", new AppEntry
                {
                    Name = "Unzip (List)",
                    Type = AppEntry.EntryType.File,
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

        foreach (var entry in entries)
        {
            var filePath = Path.Combine(Vars.AppDataDir, "entries", $"{entry.Key}.json");
            var fileContents = JsonSerializer.Serialize(entry.Value, Vars.JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        }

        foreach (var fileExtension in fileExtensions)
        {
            var filePath = Path.Combine(Vars.AppDataDir, "files", $"{fileExtension.Key}.json");
            var fileContents = JsonSerializer.Serialize(fileExtension.Value, Vars.JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        }
    }
}
