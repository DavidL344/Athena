using System.Text.Json;
using Athena.Core.Internal.Helpers;
using Athena.Desktop.Configuration;

namespace Athena.Core.Internal;

internal class Startup
{
    public static async Task CheckEntries(ConfigPaths configPaths, JsonSerializerOptions serializerOptions)
    {
        var appDataDir = configPaths.Root;
        
        if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
        
        // Check if there are any directories or files in the config directory,
        // implying whether has the application been run before or not 
        var configDirNoDirs = Directory.GetDirectories(appDataDir).Length == 0;
        var configDirNoFiles = Directory.GetFiles(appDataDir).Length == 0;
        var firstRun = configDirNoDirs && configDirNoFiles;
        
        var subDirs = configPaths.Subdirectories.Values;
        foreach (var subDir in subDirs.Where(subDir => !Directory.Exists(subDir)))
        {
            Directory.CreateDirectory(subDir);
        }
        
        if (firstRun) await SamplesHelper.Generate(configPaths, serializerOptions);
    }
}
