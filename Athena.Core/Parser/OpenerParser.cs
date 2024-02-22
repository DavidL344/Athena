using System.Text.Json;
using Athena.Core.Model.Internal;
using Athena.Core.Model.Opener;
using Athena.Core.Parser.Options;
using Athena.Core.Parser.Shared;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Parser;

public class OpenerParser
{
    private readonly ParserOptions _options;
    private readonly ILogger<OpenerParser> _logger;

    public OpenerParser(ParserOptions options, ILogger<OpenerParser> logger)
    {
        _options = options;
        _logger = logger;
    }

    internal OpenerParser(ParserOptions options)
    {
        _options = options;
        _logger = new Logger<OpenerParser>(new LoggerFactory());
    }

    public async Task<IOpener> GetOpenerDefinition(string filePath)
        => ParserHelper.IsLocalOrRequested(filePath, _options.OpenLocally, _options.StreamableProtocolPrefixes)
            ? await GetFileExtensionDefinition(filePath)
            : await GetProtocolDefinition(new Uri(filePath));

    private async Task<FileExtension> GetFileExtensionDefinition(string filePath)
    {
        _logger.LogInformation("Local file detected, getting the file extension definition...");
        
        if (Path.GetExtension(filePath).Length == 0)
            throw new ApplicationException("The file has no extension!");
        
        var fileExtension = Path.GetExtension(filePath).Substring(1);
        var definitionPath = Path.Combine(Vars.ConfigPaths[ConfigType.Files], $"{fileExtension}.json");
        
        if (!File.Exists(definitionPath))
            throw new ApplicationException($"The file extension ({fileExtension}) isn't registered with Athena!");
        
        var definitionData = await File.ReadAllTextAsync(definitionPath);
        var definition = JsonSerializer.Deserialize<FileExtension>(definitionData, Vars.JsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The file extension definition is invalid!");
        
        if (definition.AppList.Count == 0)
            throw new ApplicationException("The file extension has no associated entries!");
        
        return definition;
    }

    private async Task<Protocol> GetProtocolDefinition(Uri uri)
    {
        _logger.LogInformation("Remote file detected, getting the protocol definition...");
        
        if (!_options.AllowProtocols)
            throw new ApplicationException("The protocol handler is disabled!");
        
        var protocol = uri.Scheme;
        var definitionPath = Path.Combine(Vars.ConfigPaths[ConfigType.Protocols], $"{protocol}.json");
        
        if (!File.Exists(definitionPath))
            throw new ApplicationException($"The protocol ({protocol}) isn't registered with Athena!");
        
        var definitionData = await File.ReadAllTextAsync(definitionPath);
        var definition = JsonSerializer.Deserialize<Protocol>(definitionData, Vars.JsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The protocol definition is invalid!");

        if (definition.AppList.Count == 0)
            throw new ApplicationException("The protocol has no associated entries!");

        return definition;
    }
}
