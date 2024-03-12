using System.Text.Json;
using Athena.Core.Configuration;
using Athena.Core.Internal.Helpers;
using Athena.Core.Model;
using Athena.Core.Options;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Parser;

public class OpenerParser
{
    private readonly ConfigPaths _configPaths;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<OpenerParser> _logger;

    public OpenerParser(ConfigPaths configPaths,
        JsonSerializerOptions jsonSerializerOptions, ILogger<OpenerParser> logger)
    {
        _configPaths = configPaths;
        _jsonSerializerOptions = jsonSerializerOptions;
        _logger = logger;
    }
    
    public async Task<IOpener> GetDefinition(string parsedPath, ParserOptions options)
    {
        if (ParserHelper.IsLocalOrRequested(parsedPath, options.OpenLocally, options.StreamableProtocolPrefixes))
        {
            _logger.LogDebug("Local file detected, getting the file extension definition...");

            var fileExtension = Path.GetExtension(parsedPath);
            if (string.IsNullOrEmpty(fileExtension))
                throw new ApplicationException("The file has no extension!");
            
            return await GetFileExtensionDefinition(fileExtension);
        }
        
        _logger.LogDebug("Remote file detected, getting the protocol definition...");
        
        if (!options.AllowProtocols)
            throw new ApplicationException("The protocol handler is disabled!");
        
        return await GetProtocolDefinition(new Uri(parsedPath).Scheme);
    }

    private async Task<FileExtension> GetFileExtensionDefinition(string fileExtension)
    {
        var definitionPath = _configPaths.GetEntryPath(fileExtension, ConfigType.FileExtensions);
        
        if (!File.Exists(definitionPath))
            throw new ApplicationException($"The file extension ({fileExtension.Remove(0, 1)}) isn't registered with Athena!");
        
        var definitionData = await File.ReadAllTextAsync(definitionPath);
        var definition = JsonSerializer.Deserialize<FileExtension>(definitionData, _jsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The file extension definition is invalid!");
        
        if (definition.AppList.Count == 0)
            throw new ApplicationException("The file extension has no associated entries!");
        
        return definition;
    }

    private async Task<Protocol> GetProtocolDefinition(string uriScheme)
    {
        var definitionPath = _configPaths.GetEntryPath(uriScheme, ConfigType.Protocols);
        
        if (!File.Exists(definitionPath))
            throw new ApplicationException($"The protocol ({uriScheme}) isn't registered with Athena!");
        
        var definitionData = await File.ReadAllTextAsync(definitionPath);
        var definition = JsonSerializer.Deserialize<Protocol>(definitionData, _jsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The protocol definition is invalid!");
        
        if (definition.AppList.Count == 0)
            throw new ApplicationException("The protocol has no associated entries!");
        
        return definition;
    }
}
