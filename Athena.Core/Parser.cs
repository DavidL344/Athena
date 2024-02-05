using System.Text.Json;
using System.Text.RegularExpressions;
using Athena.Core.Internal;
using Athena.Core.Model;
using Athena.Core.Model.Entry;
using Athena.Core.Model.Opener;
using Microsoft.Extensions.Logging;

namespace Athena.Core;

public class Parser
{
    private readonly ILogger _logger;

    public Parser(ILogger<Parser> logger)
    {
        _logger = logger;
    }
    
    public async Task<IOpener> GetOpenerDefinition(string filePath, bool openLocally)
    {
        var expandedPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(filePath));
        var uri = new Uri(expandedPath);
        
        return uri.IsFile || openLocally
            ? await GetFileExtensionDefinition(uri.AbsoluteUri)
            : await GetProtocolDefinition(uri);
    }
    
    private async Task<FileExtension> GetFileExtensionDefinition(string filePath)
    {
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

        if (!definition.HasEntries())
            throw new ApplicationException("The file extension has no associated entries!");
        
        return definition;
    }
    
    private async Task<Protocol> GetProtocolDefinition(Uri uri)
    {
        if (!Vars.Config.EnableProtocolHandler)
            throw new ApplicationException("The protocol handler is disabled!");
        
        var protocol = uri.Scheme;
        var definitionPath = Path.Combine(Vars.ConfigPaths[ConfigType.Protocols], $"{protocol}.json");

        if (!File.Exists(definitionPath))
            throw new ApplicationException($"The protocol ({protocol}) isn't registered with Athena!");
        
        var definitionData = await File.ReadAllTextAsync(definitionPath);
        var definition = JsonSerializer.Deserialize<Protocol>(definitionData, Vars.JsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The protocol definition is invalid!");

        if (!definition.HasEntries())
            throw new ApplicationException("The protocol has no associated entries!");
        
        return definition;
    }

    public async Task<AppEntry> GetAppEntryDefinition<T>(
        T openerDefinition,
        int entryIndex, string filePath) where T : IOpener
    {
        filePath = Environment.ExpandEnvironmentVariables(filePath);
        
        if (!openerDefinition.HasEntry(entryIndex))
            throw new ApplicationException("The entry ID is out of range!");
        
        var appEntryName = openerDefinition.AppList[entryIndex];
        var appEntryPath = Path.Combine(Vars.ConfigPaths[ConfigType.Entries], $"{appEntryName}.json");
        
        if (!File.Exists(appEntryPath))
            throw new ApplicationException($"The entry ({appEntryName}) isn't defined!");
        
        var definitionData = await File.ReadAllTextAsync(appEntryPath);
        var definition = JsonSerializer.Deserialize<AppEntry>(definitionData, Vars.JsonSerializerOptions);
        
        if (definition is null)
            throw new ApplicationException("The entry definition is invalid!");
        
        if (definition.RemoveProtocol)
            filePath = RemoveProtocolFromUrl(filePath);
        
        return definition.ExpandEnvironmentVariables(filePath);
    }

    public async Task<AppEntry> GetFirstAppEntryDefinition<T>(
        T openerDefinition, string filePath) where T : IOpener
    {
        _logger.LogInformation("The first entry is {FirstEntry}", openerDefinition.AppList[0]);
        return await GetAppEntryDefinition(openerDefinition, 0, filePath);
    }
    
    private string RemoveProtocolFromUrl(string url)
    {
        var uri = new Uri(url);
        var protocol = uri.Scheme;
        
        var pattern = $@"(^{protocol}:(?:\/{{0,2}}))?";
        var result = Regex.Replace(url, pattern, "");
        
        return result;
    }
}
