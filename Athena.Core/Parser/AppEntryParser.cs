using System.Text.Json;
using Athena.Core.Configuration;
using Athena.Core.Internal.Helpers;
using Athena.Core.Model;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Parser;

public class AppEntryParser
{
    private readonly ConfigPaths _configPaths;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<AppEntryParser> _logger;
    
    public AppEntryParser(ConfigPaths configPaths,
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<AppEntryParser> logger)
    {
        _configPaths = configPaths;
        _jsonSerializerOptions = jsonSerializerOptions;
        _logger = logger;
    }
    
    public async Task<AppEntry> GetAppEntry(IOpener opener, int index)
    {
        if (index < 0 || index >= opener.AppList.Count)
            throw new ApplicationException("The entry ID is out of range!");
        
        var appEntryName = opener.AppList[index];
        return await GetAppEntry(appEntryName);
    }
    
    public async Task<AppEntry> GetAppEntry(string appEntryName)
    {
        if (string.IsNullOrWhiteSpace(appEntryName))
            throw new ApplicationException("The app entry name is invalid!");
        
        var appEntryPath = _configPaths.GetEntryPath(appEntryName, ConfigType.AppEntries);
        
        if (!File.Exists(appEntryPath))
            throw new ApplicationException($"The app entry ({appEntryName}) isn't registered with Athena!");
        
        _logger.LogDebug("App entry {AppEntryName} exists, reading its definition from {AppEntryPath}...",
            appEntryName, appEntryPath);
        
        var definitionData = await File.ReadAllTextAsync(appEntryPath);
        var definition = JsonSerializer.Deserialize<AppEntry>(definitionData, _jsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The app entry definition is invalid!");
        
        _logger.LogDebug("The app entry {AppEntryName} has been loaded successfully", appEntryName);

        return definition;
    }

    public AppEntry ExpandAppEntry(
        AppEntry appEntry, string argumentPath,
        IEnumerable<string> streamableProtocolPrefixes)
    {
        argumentPath = ParserHelper.ParseStreamPath(argumentPath, streamableProtocolPrefixes);
        
        _logger.LogDebug("File path has been fully parsed, the new file path is {FilePath}", argumentPath);
        
        if (appEntry.RemoveProtocol)
        {
            argumentPath = ParserHelper.RemoveProtocolFromUrl(argumentPath);
            _logger.LogDebug("Protocol removal requested, the new file path is {FilePath}", argumentPath);
        }
        
        appEntry.Path = ParserHelper.ExpandEnvironmentVariables(appEntry.Path);
        appEntry.Arguments = ParserHelper.ExpandEnvironmentVariables(appEntry.Arguments, argumentPath);
        
        _logger.LogDebug("App entry {AppEntryName} has had its variables expanded", appEntry.Name);
        
        return appEntry;
    }
    
    public async Task<string[]> GetFriendlyNames(IOpener opener, bool markDefault = false)
    {
        // Parse the names of the app entries
        var friendlyNames = new string[opener.AppList.Count];
        var defaultMark = markDefault ? " [[default]]" : string.Empty;
        
        for (var i = 0; i < opener.AppList.Count; i++)
        {
            var definition = await GetAppEntry(opener.AppList[i]);
            friendlyNames[i] = opener.AppList[i] == opener.DefaultApp
                ? $"{definition.Name}{defaultMark}"
                : definition.Name;
        }
        
        return friendlyNames;
    }
}
