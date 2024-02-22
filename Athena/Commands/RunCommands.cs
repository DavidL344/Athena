using Athena.Commands.Internal;
using Athena.CoreOld;
using Athena.CoreOld.Model.Entry;
using Athena.Terminal;
using Cocona;
using Microsoft.Extensions.Logging;

namespace Athena.Commands;

public class RunCommands : ICommands
{
    private readonly Parser _parser;
    private readonly Runner _runner;
    private readonly ILogger<RunCommands> _logger;
    
    public RunCommands(Parser parser, Runner runner, ILogger<RunCommands> logger)
    {
        _parser = parser;
        _runner = runner;
        _logger = logger;
    }
    
    [Command("run", Description = "Open a file with the registered application")]
    public async Task<int> OpenFile(
        [Argument("file", Description = "A file to be opened")] string filePath,
        [Option('e', Description = "An entry ID from the list of registered apps")] int? entry,
        [Option('f', Description = "Skip the app picker and choose the top-most option")] bool first,
        [Option('l', Description = "Open any possible URLs as files rather than links")] bool local,
        [Option('p', Description = "Show the app picker even when the default app is specified")] bool picker)
    {
        _logger.LogInformation(
            "Opening {File} with entry={Entry}, default={PickDefaultApp}, local={Local}, and picker={Picker}...",
            filePath, entry, !first, local, picker);
        
        try
        {
            var appEntryDefinition = await GetAppDefinition(filePath, entry, first, local, picker);
            return await _runner.Run(appEntryDefinition.Path, appEntryDefinition.Arguments);
        }
        catch (Exception e)
        {
            _logger.LogError("An error occurred while opening the file: {Message}", e.Message);
            return 1;
        }
    }

    private async Task<AppEntry> GetAppDefinition(string filePath, int? entry, bool first, bool local, bool picker)
    {
        var openerDefinition = await _parser.GetOpenerDefinition(filePath, local);
        
        // The user has picked an entry ID to open the file/protocol with
        if (entry is not null)
            return await _parser.GetAppEntryDefinition(openerDefinition, entry.Value, filePath);
        
        // The user wants to use the first app in the list
        if (first)
            return await _parser.GetFirstAppEntryDefinition(openerDefinition, filePath);
        
        // There's no default app specified or the user decides to pick an app at runtime
        if (picker || string.IsNullOrWhiteSpace(openerDefinition.DefaultApp))
        {
            var appIndex = AppPicker.Show(openerDefinition);
            
            if (appIndex == -1)
                throw new ApplicationException("The user has cancelled the operation!");
            
            return await _parser.GetAppEntryDefinition(openerDefinition, appIndex, filePath);
        }
        
        // When no options are specified, use the default app
        var defaultEntry = openerDefinition.AppList.ToList().IndexOf(openerDefinition.DefaultApp);
        if (defaultEntry == -1)
            throw new ApplicationException("The default app is not in the list of registered apps!");
        
        return await _parser.GetAppEntryDefinition(openerDefinition, defaultEntry, filePath);
    }
}
