using Athena.Cli.Commands.Internal;
using Athena.Cli.Model;
using Athena.Cli.Terminal;
using Athena.Core.Configuration;
using Athena.Core.Extensions;
using Athena.Core.Model;
using Athena.Core.Options;
using Athena.Core.Parser;
using Athena.Core.Runner;
using Cocona;
using Microsoft.Extensions.Logging;

namespace Athena.Cli.Commands;

public class RunCommands : ICommands
{
    private readonly Config _config;
    private readonly PathParser _pathParser;
    private readonly OpenerParser _openerParser;
    private readonly AppEntryParser _appEntryParser;
    private readonly AppRunner _runner;
    private readonly ILogger<RunCommands> _logger;
    
    public RunCommands(Config config, PathParser pathParser,
        OpenerParser openerParser, AppEntryParser appEntryParser,
        AppRunner runner, ILogger<RunCommands> logger)
    {
        _config = config;
        _pathParser = pathParser;
        _openerParser = openerParser;
        _appEntryParser = appEntryParser;
        _runner = runner;
        _logger = logger;
    }

    [Command("run", Description = "Open a file with the registered application")]
    public async Task<int> OpenFile(
        [Argument("file", Description = "A file to be opened")] string filePath,
        [Option('e', Description = "An entry name from the list of registered apps")] string? entry,
        [Option('i', Description = "An entry ID from the list of registered apps")] int? id,
        [Option('f', Description = "Skip the app picker and choose the top-most option")] bool first,
        [Option('l', Description = "Open any possible URLs as files rather than links")] bool local,
        [Option('p', Description = "Show the app picker even when the default app is specified")] bool picker)
    {
        _logger.LogDebug(
            "Opening {File} with entry={Entry}, default={PickDefaultApp}, local={Local}, and picker={Picker}...",
            filePath, entry, !first, local, picker);

        var openFileOptions = new OpenFileOptions
        {
            FilePath = filePath,
            EntryName = entry,
            EntryId = id,
            OpenWithTheFirstEntry = first,
            OpenLocally = local,
            ForceShowAppPicker = picker
        };
        
        var parserOptions = new ParserOptions
        {
            AllowProtocols = _config.EnableProtocolHandler,
            OpenLocally = local,
            StreamableProtocolPrefixes = _config.StreamableProtocolPrefixes
        };
        
        try
        {
            var parsedPath = _pathParser.GetPath(filePath, parserOptions);
            var opener = _openerParser.GetDefinition(parsedPath, parserOptions);
            var index = await GetAppEntryIndex(opener, openFileOptions);
            
            if (index == -1)
                throw new ApplicationException("The default app is not in the list of registered apps!");
            
            var appEntry = _appEntryParser.GetAppEntry(opener, index);
            var expandedAppEntry = _appEntryParser.ExpandAppEntry(
                appEntry, parsedPath, parserOptions.StreamableProtocolPrefixes);
            
            return await _runner.RunAsync(expandedAppEntry.Path, expandedAppEntry.Arguments);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred while opening the file: {Message}", e.Message);
            return 1;
        }
    }
    
    private async Task<int> GetAppEntryIndex(IOpener opener, OpenFileOptions openFileOptions)
    {
        int entryIndex;
        
        // The user has picked an entry name to open the file/protocol with
        if (openFileOptions.EntryName is not null)
            return opener.GetAppEntryIndex(openFileOptions.EntryName);
            
        // The user has picked an entry ID to open the file/protocol with
        if (openFileOptions.EntryId is not null)
            return openFileOptions.EntryId.Value;
            
        // The user wants to use the first app in the list
        if (openFileOptions.OpenWithTheFirstEntry)
            return 0;
            
        // There's no default app specified or the user decides to pick an app at runtime
        if (openFileOptions.ForceShowAppPicker || string.IsNullOrWhiteSpace(opener.DefaultApp))
        {
            // Parse the app entries' names
            var appList = _appEntryParser.GetFriendlyNames(opener, true);
            entryIndex = await AppPicker.Show(opener.Name, appList);
                
            if (entryIndex == -1)
                throw new ApplicationException("The user has cancelled the operation!");

            return entryIndex;
        }
            
        // When no options are specified, use the default app
        entryIndex = opener.GetAppEntryIndex(opener.DefaultApp);
        
        if (entryIndex == -1)
            _logger.LogDebug("Found {EntryName} at index {EntryIndex}",
                openFileOptions.EntryName, entryIndex);
        
        return entryIndex;
    }
}
