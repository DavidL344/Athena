using System.Text.Json;
using Athena.Core.Internal;
using Athena.Core.Model;
using Microsoft.Extensions.Logging;

namespace Athena.Core;

public class Parser(ILogger logger)
{
    public async Task<FileExtension> GetFileExtensionDefinition(string filePath)
    {
        filePath = Environment.ExpandEnvironmentVariables(filePath);
        
        if (Path.GetExtension(filePath).Length == 0)
            throw new ApplicationException("The file has no extension!");
        
        var fileExtension = Path.GetExtension(filePath).Substring(1);
        var definitionPath = Path.Combine(Vars.AppDataDir, "files", $"{fileExtension}.json");

        if (!File.Exists(definitionPath))
            throw new ApplicationException($"The file extension ({fileExtension}) isn't registered with Athena!");
        
        var definitionData = await File.ReadAllTextAsync(definitionPath);
        var definition = JsonSerializer.Deserialize<FileExtension>(definitionData, Vars.JsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The file extension definition is invalid!");

        if (!definition.HasEntries())
            throw new ApplicationException("The file extension has no associated entries!");
        
        return definition;
    }

    public async Task<AppEntry> GetAppEntryDefinition(FileExtension fileExtensionDefinition, int entryIndex)
    {
        if (!fileExtensionDefinition.HasEntry(entryIndex))
            throw new ApplicationException("The entry ID is out of range!");
        
        var appEntryName = fileExtensionDefinition.AppList[entryIndex];
        var appEntryPath = Path.Combine(Vars.AppDataDir, "entries", $"{appEntryName}.json");
        
        if (!File.Exists(appEntryPath))
            throw new ApplicationException($"The entry ({appEntryName}) isn't defined!");
        
        var definitionData = await File.ReadAllTextAsync(appEntryPath);
        var definition = JsonSerializer.Deserialize<AppEntry>(definitionData, Vars.JsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The entry definition is invalid!");

        return definition;
    }

    public async Task<AppEntry> GetFirstAppEntryDefinition(FileExtension fileExtensionDefinition, string filePath)
    {
        logger.LogInformation("The first entry is {FirstEntry}", fileExtensionDefinition.AppList[0]);
        return await GetAppEntryDefinition(fileExtensionDefinition, 0, filePath);
    }

    public async Task<AppEntry> GetAppEntryDefinition(
        FileExtension fileExtensionDefinition, int entryIndex,
        string filePath)
    {
        filePath = Environment.ExpandEnvironmentVariables(filePath);
        var definition = await GetAppEntryDefinition(fileExtensionDefinition, entryIndex);
        return definition.ExpandEnvironmentVariables(filePath);
    }
}
