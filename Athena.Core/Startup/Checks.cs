using System.Text.Json;
using Athena.Core.Model;
using Athena.Core.Parser.Shared;
using Athena.Core.Samples;

namespace Athena.Core.Startup;

internal class Checks
{
    public static async Task CheckConfiguration(string appDataDir, JsonSerializerOptions serializerOptions)
    {
        if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
        
        var configDirNoDirs = Directory.GetDirectories(appDataDir).Length == 0;
        var configDirNoFiles = Directory.GetFiles(appDataDir).Length == 0;
        var firstRun = configDirNoDirs && configDirNoFiles;
        
        if (!File.Exists(Path.Combine(appDataDir, "config.json")))
        {
            var fileContents = JsonSerializer.Serialize(new Config(), serializerOptions);
            await File.WriteAllTextAsync(Path.Combine(appDataDir, "config.json"), fileContents);
        }
        
        var subDirs = new[] { "entries", "files", "protocols" };
        foreach (var subDir in subDirs)
        {
            var path = Path.Combine(appDataDir, subDir);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        
        if (firstRun) await GenerateSamples(appDataDir, serializerOptions);
    }
    
    private static async Task GenerateSamples(string appDataDir, JsonSerializerOptions serializerOptions)
    {
        var samples = new List<ISample>
        {
            new ArchiveSamples(),
            new MediaSamples(),
            new WebSamples()
        };
        
        var configPaths = new Dictionary<ConfigType, string>
        {
            { ConfigType.Entries, Path.Combine(appDataDir, "entries") },
            { ConfigType.Files, Path.Combine(appDataDir, "files") },
            { ConfigType.Protocols, Path.Combine(appDataDir, "protocols") }
        };
        
        var tasks = samples.Select(async sample =>
            await sample.SaveToDisk(configPaths, serializerOptions));
        await Task.WhenAll(tasks);
    }
}