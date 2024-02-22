using System.Text.Json;
using Athena.Core.Model.Internal;
using Athena.Core.Model.Opener;
using Athena.Core.Parser.Options;
using Athena.Core.Parser.Shared;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Parser;

public class AppParser
{
    private readonly ParserOptions _options;
    private readonly ILogger<AppParser> _logger;

    public AppParser(ParserOptions options, ILogger<AppParser> logger)
    {
        _options = options;
        _logger = logger;
    }
    
    internal AppParser(ParserOptions options)
    {
        _options = options;
        _logger = new Logger<AppParser>(new LoggerFactory());
    }

    public async Task<AppEntry> GetAppDefinition<T>(
        T openerDefinition, string filePath, string entryName, bool expandVars = true)
        where T : IOpener
    {
        var entryIndex = openerDefinition.AppList.IndexOf(entryName);
        if (entryIndex == -1)
            throw new ApplicationException(
                $"The app entry ({entryName}) isn't registered with the opener ({openerDefinition.Name})!");
        
        _logger.LogInformation("Found {EntryName} at index {EntryIndex}", entryName, entryIndex);
        
        return await GetAppDefinition(openerDefinition, filePath, entryIndex, expandVars);
    }
    
    public async Task<AppEntry> GetAppDefinition<T>(
        T openerDefinition, string filePath, int entryIndex, bool expandVars = true)
        where T : IOpener
    {
        if (filePath.Length == 0)
            throw new ApplicationException("The file path is empty!");
        
        if (entryIndex < 0 || entryIndex >= openerDefinition.AppList.Count)
            throw new ApplicationException("The entry ID is out of range!");
        
        var appEntryName = openerDefinition.AppList[entryIndex];
        var appEntryPath = Path.Combine(Vars.ConfigPaths[ConfigType.Entries], $"{appEntryName}.json");
        
        if (!File.Exists(appEntryPath))
            throw new ApplicationException($"The entry ({appEntryName}) isn't defined!");
        
        _logger.LogInformation("Entry {AppEntryName} exists, reading its definition from {AppEntryPath}...",
            appEntryName, appEntryPath);
        
        var definitionData = await File.ReadAllTextAsync(appEntryPath);
        var definition = JsonSerializer.Deserialize<AppEntry>(definitionData, Vars.JsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The entry definition is invalid!");
        
        _logger.LogInformation("Entry {AppEntryName} has been loaded successfully", appEntryName);
        
        filePath = ParserHelper.ParseStreamPath(filePath, _options.StreamableProtocolPrefixes);
        
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
}
