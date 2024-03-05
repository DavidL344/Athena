using Athena.Commands.Internal;
using Athena.Commands.Internal.Model;
using Athena.Core.Model;
using Athena.Core.Model.AppPicker;
using Athena.Core.Model.Opener;
using Athena.Core.Parser;
using Athena.Core.Parser.Options;
using Athena.Core.Runner;
using Athena.Terminal;
using Cocona;
using Microsoft.Extensions.Logging;

namespace Athena.Commands;

public class RunCommands : ICommands
{
    private readonly Config _config;
    private readonly PathParser _pathParser;
    private readonly OpenerParser _openerParser;
    private readonly AppParser _appParser;
    private readonly AppRunner _runner;
    private readonly ILogger<RunCommands> _logger;
    
    public RunCommands(Config config, PathParser pathParser,
        OpenerParser openerParser, AppParser appParser,
        AppRunner runner, ILogger<RunCommands> logger)
    {
        _config = config;
        _pathParser = pathParser;
        _openerParser = openerParser;
        _appParser = appParser;
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
        _logger.LogInformation(
            "Opening {File} with entry={Entry}, default={PickDefaultApp}, local={Local}, and picker={Picker}...",
            filePath, entry, !first, local, picker);

        var options = new OpenFileOptions
        {
            FilePath = filePath,
            EntryName = entry,
            EntryId = id,
            OpenWithTheFirstEntry = first,
            OpenLocally = local,
            ForceShowAppPicker = picker
        };
        
        try
        {
            var appEntryDefinition = await GetAppDefinition(options);
            return await _runner.RunAsync(appEntryDefinition.Path, appEntryDefinition.Arguments);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred while opening the file: {Message}", e.Message);
            return 1;
        }
    }

    private async Task<AppEntry> GetAppDefinition(OpenFileOptions openFileOptions)
    {
        var options = new ParserOptions
        {
            OpenLocally = openFileOptions.OpenLocally,
            AllowProtocols = _config.EnableProtocolHandler,
            StreamableProtocolPrefixes = _config.StreamableProtocolPrefixes
        };
        
        var parsedPath = _pathParser.GetPath(openFileOptions.FilePath, options);
        var openerDefinition = await _openerParser.GetOpenerDefinition(parsedPath, options);
        
        // The user has picked an entry name to open the file/protocol with
        if (openFileOptions.EntryName is not null)
            return await _appParser.GetAppDefinition(
                openerDefinition, openFileOptions.FilePath, openFileOptions.EntryName, options);
        
        // The user has picked an entry ID to open the file/protocol with
        if (openFileOptions.EntryId is not null)
            return await _appParser.GetAppDefinition(
                openerDefinition, openFileOptions.FilePath, openFileOptions.EntryId.Value, options);
        
        // The user wants to use the first app in the list
        if (openFileOptions.OpenWithTheFirstEntry)
            return await _appParser.GetAppDefinition(
                openerDefinition, openFileOptions.FilePath, 0, options);
        
        // There's no default app specified or the user decides to pick an app at runtime
        if (openFileOptions.ForceShowAppPicker || string.IsNullOrWhiteSpace(openerDefinition.DefaultApp))
        {
            var context = new AppPickerContext
            {
                FriendlyName = openerDefinition.Name,
                FilePath = openFileOptions.FilePath,
                AppEntries = await _appParser.GetFriendlyNames(openerDefinition)
            };
            var appIndex = await AppPicker.Show(context);
            
            if (appIndex == -1)
                throw new ApplicationException("The user has cancelled the operation!");
            
            return await _appParser.GetAppDefinition(openerDefinition, openFileOptions.FilePath, appIndex, options);
        }
        
        // When no options are specified, use the default app
        var defaultEntry = openerDefinition.AppList.ToList().IndexOf(openerDefinition.DefaultApp);
        if (defaultEntry == -1)
            throw new ApplicationException("The default app is not in the list of registered apps!");
        
        return await _appParser.GetAppDefinition(openerDefinition, parsedPath, defaultEntry, options);
    }
}
