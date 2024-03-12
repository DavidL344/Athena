using System.Text.Json;
using Athena.Cli.Commands.Internal;
using Athena.Core.Configuration;
using Athena.Core.Model;
using Athena.Core.Parser;
using Athena.Core.Runner;
using Cocona;
using Microsoft.Extensions.Logging;

namespace Athena.Cli.Commands;

public class ConfigCommands : ICommands
{
    private readonly Config _config;
    private readonly ConfigPaths _configPaths;
    private readonly AppEntryParser _appEntryParser;
    private readonly AppRunner _appRunner;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<ConfigCommands> _logger;
    
    public ConfigCommands(Config config, ConfigPaths configPaths,
        AppEntryParser appEntryParser, AppRunner appRunner,
        JsonSerializerOptions serializerOptions, ILogger<ConfigCommands> logger)
    {
        _config = config;
        _configPaths = configPaths;
        _appEntryParser = appEntryParser;
        _appRunner = appRunner;
        _jsonSerializerOptions = serializerOptions;
        _logger = logger;
    }
    
    [Command("edit", Description = "Edit the configuration")]
    public async Task EditConfig(
        [Argument("file", Description = "An entry (app entry, file extension, protocol) to be edited")] string? entry,
        [Option(Description = "Overrides the default editor specified in the configuration")] string? editor)
    {
        _logger.LogWarning("Please note that this command only fully supports GUI editors!");
        
        editor ??= _config.Editor;
        var editPath = await GetEditPath(entry);
        
        var appEntry = await _appEntryParser.GetAppEntry(editor);
        var expandedAppEntry = _appEntryParser.ExpandAppEntry(
            appEntry, editPath, _config.StreamableProtocolPrefixes);
        
        await _appRunner.RunAsync(expandedAppEntry.Path, expandedAppEntry.Arguments);
    }

    private async Task<string> GetEditPath(string? entry)
    {
        string editPath;
        
        // Edit the config file
        if (entry is null)
        {
            _logger.LogDebug("Parsed {Entry} as the config file", entry);
            
            return _configPaths.ConfigFile;
        }
        
        // Edit a file extension
        if (entry.StartsWith('.'))
        {
            _logger.LogDebug("Parsed {Entry} as a file extension", entry);
            
            editPath = _configPaths.GetEntryPath(entry, ConfigType.FileExtensions);
            
            if (!File.Exists(editPath))
            {
                var fileContents = new FileExtension
                {
                    Name = $"{_configPaths.GetParsedEntryName(entry, ConfigType.FileExtensions).ToUpper()} file",
                    AppList = ["sample.open", "sample.edit"],
                    DefaultApp = ""
                };
                
                await File.WriteAllTextAsync(editPath,
                    JsonSerializer.Serialize(fileContents, _jsonSerializerOptions));
            }

            return editPath;
        }
        
        // Edit a protocol
        if (entry.Contains("://"))
        {
            _logger.LogDebug("Parsed {Entry} as a protocol", entry);
            
            editPath = _configPaths.GetEntryPath(entry, ConfigType.Protocols);

            if (!File.Exists(editPath))
            {
                var protocolContents = new Protocol
                {
                    Name = _configPaths.GetParsedEntryName(entry, ConfigType.Protocols),
                    AppList = ["sample.open", "sample.edit"],
                    DefaultApp = ""
                };
                
                await File.WriteAllTextAsync(editPath,
                    JsonSerializer.Serialize(protocolContents, _jsonSerializerOptions));
            }
                
            return editPath;
        }
        
        // Edit an app entry
        _logger.LogDebug("Parsed {Entry} as an app entry", entry);
        
        editPath = _configPaths.GetEntryPath(entry, ConfigType.AppEntries);
        if (File.Exists(editPath)) return editPath;
        
        var appContents = new AppEntry
        {
            Name = _configPaths.GetParsedEntryName(entry, ConfigType.AppEntries),
            Path = "sample",
            Arguments = "$FILE",
            RemoveProtocol = false
        };
            
        await File.WriteAllTextAsync(editPath,
            JsonSerializer.Serialize(appContents, _jsonSerializerOptions));

        return editPath;
    }
}
