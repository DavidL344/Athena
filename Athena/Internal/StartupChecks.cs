using System.Text.Json;
using Athena.Model;

namespace Athena.Internal;

public static class StartupChecks
{
    public static async Task RunAsync()
    {
        var firstRun = !Directory.Exists(Vars.AppDataDir);
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

        foreach (var entry in entries)
        {
            var filePath = Path.Combine(Vars.AppDataDir, "entries", $"{entry.Key}.json");
            var fileContents = JsonSerializer.Serialize(entry.Value, Vars.JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        }
    }
}
