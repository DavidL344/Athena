using System.Text.Json;
using Athena.Core.Internal.Samples;
using Athena.Core.Model.Configuration;

namespace Athena.Core.Internal;

internal static class StartupChecks
{
    public static async Task RunAsync()
    {
        var configDirNoDirs = Directory.GetDirectories(Vars.AppDataDir).Length == 0;
        var configDirNoFiles = Directory.GetFiles(Vars.AppDataDir).Length == 0;
        
        var firstRun = !Directory.Exists(Vars.AppDataDir) || (configDirNoDirs && configDirNoFiles);
        if (firstRun) Directory.CreateDirectory(Vars.AppDataDir);

        if (!File.Exists(Vars.AppConfigPath))
        {
            var fileContents = JsonSerializer.Serialize(new Config(), Vars.JsonSerializerOptions);
            await File.WriteAllTextAsync(Vars.AppConfigPath, fileContents);
        }
        
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
        var samples = new List<ISample>
        {
            new ArchiveSamples(),
            new MediaSamples(),
            new WebSamples()
        };
        
        var tasks = samples.Select(async sample => await sample.SaveToDisk());
        await Task.WhenAll(tasks);
    }
}
