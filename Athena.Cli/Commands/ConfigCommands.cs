using System.Text.Json;
using Athena.Cli.Commands.Internal;
using Athena.Core.Configuration;
using Athena.Core.Model.Opener;
using Athena.Core.Parser;
using Athena.Core.Parser.Shared;
using Athena.Core.Runner;
using Cocona;
using Microsoft.Extensions.Logging;

namespace Athena.Cli.Commands;

public class ConfigCommands : ICommands
{
    private readonly Config _config;
    private readonly ConfigPaths _configPaths;
    private readonly AppParser _appParser;
    private readonly AppRunner _appRunner;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<ConfigCommands> _logger;
    
    public ConfigCommands(Config config, ConfigPaths configPaths,
        AppParser appParser, AppRunner appRunner,
        JsonSerializerOptions serializerOptions, ILogger<ConfigCommands> logger)
    {
        _config = config;
        _configPaths = configPaths;
        _appParser = appParser;
        _appRunner = appRunner;
        _jsonSerializerOptions = serializerOptions;
        _logger = logger;
    }
    
    [Command("edit", Description = "Edit the configuration")]
    public async Task EditConfig(
        [Argument("file", Description = "An entry (app entry, file extension, protocol) to be edited")] string? entry,
        [Option(Description = "Overrides the default editor specified in the configuration")] string? editor)
    {
        editor ??= _config.Editor;
        string editPath;
        
        if (entry is null)
        {
            var config = await _appParser.GetAppDefinition(editor, _configPaths.File);
            await _appRunner.RunAsync(config.Path, config.Arguments);
            return;
        }
        
        if (entry.StartsWith('.'))
        {
            editPath = Path.Combine(_configPaths.Subdirectories[ConfigType.Files],
                $"{entry.Remove(0, 1)}.json");
            
            if (!File.Exists(editPath))
            {
                var fileContents = new FileExtension
                {
                    Name = $"{entry.Remove(0, 1).ToUpper()} file",
                    AppList = ["sample.open", "sample.edit"],
                    DefaultApp = ""
                };
                
                await File.WriteAllTextAsync(editPath,
                    JsonSerializer.Serialize(fileContents, _jsonSerializerOptions));
            }
            
            var fileExtension = await _appParser.GetAppDefinition(editor, editPath);
            await _appRunner.RunAsync(fileExtension.Path, fileExtension.Arguments);
            return;
        }

        if (entry.Contains("://"))
        {
            editPath = Path.Combine(_configPaths.Subdirectories[ConfigType.Protocols],
                $"{entry.Replace("://", "")}.json");

            if (!File.Exists(editPath))
            {
                var protocolContents = new Protocol
                {
                    Name = entry.Replace("://", ""),
                    AppList = ["sample.open", "sample.edit"],
                    DefaultApp = ""
                };
                
                await File.WriteAllTextAsync(editPath,
                    JsonSerializer.Serialize(protocolContents, _jsonSerializerOptions));
            }
            
            var protocol = await _appParser.GetAppDefinition(editor, editPath);
            await _appRunner.RunAsync(protocol.Path, protocol.Arguments);
            return;
        }
        
        _logger.LogInformation("Editing {File}...", entry);
        editPath = Path.Combine(_configPaths.Subdirectories[ConfigType.Entries], $"{entry}.json");
        if (!File.Exists(editPath))
        {
            var appContents = new AppEntry
            {
                Name = entry,
                Path = "sample",
                Arguments = "$FILE",
                RemoveProtocol = false
            };
            
            await File.WriteAllTextAsync(editPath,
                JsonSerializer.Serialize(appContents, _jsonSerializerOptions));
        }
        
        var appDefinition = await _appParser.GetAppDefinition(editor, editPath);
        await _appRunner.RunAsync(appDefinition.Path, appDefinition.Arguments);
    }
}
