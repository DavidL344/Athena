using System.Text.Json;
using Athena.Core.Configuration;
using Athena.Core.Model.Internal;
using Athena.Core.Model.Opener;
using Athena.Core.Parser.Options;
using Athena.Core.Parser.Shared;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Parser;

public class AppParser
{
    private readonly ConfigPaths _configPaths;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<AppParser> _logger;

    public AppParser(ConfigPaths configPaths,
        JsonSerializerOptions jsonSerializerOptions, ILogger<AppParser> logger)
    {
        _configPaths = configPaths;
        _jsonSerializerOptions = jsonSerializerOptions;
        _logger = logger;
    }

    public async Task<AppEntry> GetAppDefinition<T>(
        T openerDefinition, string filePath, string entryName, ParserOptions options, bool expandVars = true)
        where T : IOpener
    {
        var entryIndex = openerDefinition.AppList.IndexOf(entryName);
        if (entryIndex == -1)
            throw new ApplicationException(
                $"The app entry ({entryName}) isn't registered with the opener ({openerDefinition.Name})!");
        
        _logger.LogInformation("Found {EntryName} at index {EntryIndex}", entryName, entryIndex);
        
        return await GetAppDefinition(openerDefinition, filePath, entryIndex, options, expandVars);
    }
    
    public async Task<AppEntry> GetAppDefinition<T>(
        T openerDefinition, string filePath, int entryIndex, ParserOptions options, bool expandVars = true)
        where T : IOpener
    {
        if (filePath.Length == 0)
            throw new ApplicationException("The file path is empty!");
        
        if (entryIndex < 0 || entryIndex >= openerDefinition.AppList.Count)
            throw new ApplicationException("The entry ID is out of range!");
        
        var appEntryName = openerDefinition.AppList[entryIndex];
        var definition = await GetAppDefinition(appEntryName);
        
        filePath = ParserHelper.ParseStreamPath(filePath, options.StreamableProtocolPrefixes);
        
        _logger.LogInformation("File path has been fully parsed, the new file path is {FilePath}", filePath);
        
        if (definition.RemoveProtocol)
        {
            filePath = ParserHelper.RemoveProtocolFromUrl(filePath);
            _logger.LogInformation("Protocol removal requested, the new file path is {FilePath}", filePath);
        }

        if (!expandVars) return definition;
        
        definition.Path = ParserHelper.ExpandEnvironmentVariables(definition.Path);
        definition.Arguments = ParserHelper.ExpandEnvironmentVariables(definition.Arguments, filePath);
        
        _logger.LogInformation("App entry {AppEntryName} has had its variables expanded", appEntryName);

        return definition;
    }

    public async Task<AppEntry> GetAppDefinition(string definitionName)
    {
        var appEntryPath = Path.Combine(_configPaths.Subdirectories[ConfigType.Entries], $"{definitionName}.json");
        
        if (!File.Exists(appEntryPath))
            throw new ApplicationException($"The entry ({definitionName}) isn't defined!");
        
        _logger.LogInformation("Entry {AppEntryName} exists, reading its definition from {AppEntryPath}...",
            definitionName, appEntryPath);
        
        var definitionData = await File.ReadAllTextAsync(appEntryPath);
        var definition = JsonSerializer.Deserialize<AppEntry>(definitionData, _jsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The entry definition is invalid!");
        
        _logger.LogInformation("Entry {AppEntryName} has been loaded successfully", definitionName);

        return definition;
    }
    
    public async Task<AppEntry> GetAppDefinition(string definitionName, string argumentPath)
    {
        var appEntryPath = Path.Combine(_configPaths.Subdirectories[ConfigType.Entries], $"{definitionName}.json");
        
        if (!File.Exists(appEntryPath))
            throw new ApplicationException($"The entry ({definitionName}) isn't defined!");
        
        _logger.LogInformation("Entry {AppEntryName} exists, reading its definition from {AppEntryPath}...",
            definitionName, appEntryPath);
        
        var definitionData = await File.ReadAllTextAsync(appEntryPath);
        var definition = JsonSerializer.Deserialize<AppEntry>(definitionData, _jsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The entry definition is invalid!");
        
        _logger.LogInformation("Entry {AppEntryName} has been loaded successfully", definitionName);
        
        definition.Path = ParserHelper.ExpandEnvironmentVariables(definition.Path);
        definition.Arguments = ParserHelper.ExpandEnvironmentVariables(definition.Arguments, argumentPath);
        
        _logger.LogInformation("App entry {AppEntryName} has had its variables expanded", definitionName);

        return definition;
    }
    
    public async Task<string[]> GetFriendlyNames(IOpener opener)
    {
        // Parse the names of the app entries
        var friendlyNames = new string[opener.AppList.Count];
        
        for (var i = 0; i < opener.AppList.Count; i++)
        {
            var entryDefinition = await GetAppDefinition(opener.AppList[i]);
            friendlyNames[i] = opener.AppList[i] == opener.DefaultApp
                ? $"{entryDefinition.Name} [[default]]"
                : entryDefinition.Name;
        }
        
        return friendlyNames;
    }
}
