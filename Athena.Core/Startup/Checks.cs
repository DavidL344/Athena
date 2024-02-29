using System.Text.Json;
using Athena.Core.Parser.Shared;
using Athena.Core.Samples;

namespace Athena.Core.Startup;

internal class Checks
{
    public static async Task CheckEntries(string appDataDir, JsonSerializerOptions serializerOptions)
    {
        if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
        
        // Check if there are any directories or files in the config directory,
        // implying whether has the application been run before or not 
        var configDirNoDirs = Directory.GetDirectories(appDataDir).Length == 0;
        var configDirNoFiles = Directory.GetFiles(appDataDir).Length == 0;
        var firstRun = configDirNoDirs && configDirNoFiles;
        
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
