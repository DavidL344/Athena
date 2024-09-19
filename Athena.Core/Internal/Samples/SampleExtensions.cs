using System.Text.Json;
using Athena.Desktop.Configuration;

namespace Athena.Core.Internal.Samples;

internal static class SampleExtensions
{
    public static async Task SaveToDisk(this ISample sample,
        ConfigPaths configPaths, JsonSerializerOptions serializerOptions)
    {
        var entryTasks = sample.Entries.Select(async entry =>
        {
            var filePath = configPaths.GetEntryPath(entry.Key, ConfigType.AppEntries);
            var fileContents = JsonSerializer.Serialize(entry.Value, serializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        });
        await Task.WhenAll(entryTasks);
        
        var fileExtensionTasks = sample.FileExtensions.Select(async fileExtension =>
        {
            var filePath = configPaths.GetEntryPath(fileExtension.Key, ConfigType.FileExtensions);
            var fileContents = JsonSerializer.Serialize(fileExtension.Value, serializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        });
        await Task.WhenAll(fileExtensionTasks);
        
        var protocolTasks = sample.Protocols.Select(async protocol =>
        {
            var filePath = configPaths.GetEntryPath(protocol.Key, ConfigType.Protocols);
            var fileContents = JsonSerializer.Serialize(protocol.Value, serializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        });
        await Task.WhenAll(protocolTasks);
    }
}
