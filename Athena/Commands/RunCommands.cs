using Athena.Core;
using Athena.Core.Model.Entry;
using Cocona;
using Microsoft.Extensions.Logging;

namespace Athena.Commands;

public class RunCommands : CoconaConsoleAppBase
{
    [Command("run", Description = "Open a file with the registered application")]
    public async Task<int> OpenFile(
        [Argument("file", Description = "A file to be opened")] string filePath,
        [Option('e', Description = "An entry ID from the list of registered apps")] int? entry,
        [Option('f', Description = "Skip the app picker and choose the top-most option")] bool first,
        [Option('l', Description = "Open any possible URLs as files rather than links")] bool local,
        [Option('p', Description = "Show the app picker even when the default app is specified")] bool picker)
    {
        Context.Logger.LogInformation(
            "Opening {File} with entry={Entry}, default={PickDefaultApp}, local={Local}, and picker={Picker}...",
            filePath, entry, !first, local, picker);
        
        try
        {
            var appEntryDefinition = await GetAppDefinition(filePath, entry, first, local, picker);
            var runner = new Runner(Context.Logger);
            return await runner.Run(appEntryDefinition.Path, appEntryDefinition.Arguments);
        }
        catch (Exception e)
        {
            Context.Logger.LogError("An error occurred while opening the file: {Message}", e.Message);
            return 1;
        }
    }

    private async Task<AppEntry> GetAppDefinition(string filePath, int? entry, bool first, bool local, bool picker)
    {
        var parser = new Parser(Context.Logger);
        var openerDefinition = await parser.GetOpenerDefinition(filePath, local);
        
        // The user has picked an entry ID to open the file/protocol with
        if (entry is not null)
            return await parser.GetAppEntryDefinition(openerDefinition, entry.Value, filePath);
        
        // The user wants to use the default app
        if (first)
            return await parser.GetFirstAppEntryDefinition(openerDefinition, filePath);
        
        // The user decides to pick an app at runtime
        if (picker)
            // TODO: open the app picker
            throw new NotImplementedException("The app picker has yet to be implemented!");

        if (string.IsNullOrWhiteSpace(openerDefinition.DefaultApp))
        {
            // TODO: open the app picker
            Context.Logger.LogWarning("There's no default app for the file extension and the picker is not yet implemented");
            return await parser.GetFirstAppEntryDefinition(openerDefinition, filePath);
        }
        
        // When no options are specified, use the default app
        var defaultEntry = openerDefinition.AppList.ToList().IndexOf(openerDefinition.DefaultApp);
        if (defaultEntry == -1)
            throw new ApplicationException("The default app is not in the list of registered apps!");
        
        return await parser.GetAppEntryDefinition(openerDefinition, defaultEntry, filePath);
    }
}
