using System.Text.Json;
using Athena.Core.Parser.Shared;

namespace Athena.Core.Samples;

public static class SampleExtensions
{
    public static async Task SaveToDisk(this ISample sample, Dictionary<ConfigType, string> configPaths)
    {
        var entryTasks = sample.Entries.Select(async entry =>
        {
            var filePath = Path.Combine(configPaths[ConfigType.Entries], $"{entry.Key}.json");
            var fileContents = JsonSerializer.Serialize(entry.Value, Vars.JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        });
        await Task.WhenAll(entryTasks);
        
        var fileExtensionTasks = sample.FileExtensions.Select(async fileExtension =>
        {
            var filePath = Path.Combine(configPaths[ConfigType.Files], $"{fileExtension.Key}.json");
            var fileContents = JsonSerializer.Serialize(fileExtension.Value, Vars.JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        });
        await Task.WhenAll(fileExtensionTasks);
        
        var protocolTasks = sample.Protocols.Select(async protocol =>
        {
            var filePath = Path.Combine(configPaths[ConfigType.Protocols], $"{protocol.Key}.json");
            var fileContents = JsonSerializer.Serialize(protocol.Value, Vars.JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, fileContents);
        });
        await Task.WhenAll(protocolTasks);
    }
}
