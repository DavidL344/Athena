using Athena.Core;
using Athena.Core.Model;
using Cocona;
using Microsoft.Extensions.Logging;

namespace Athena.Commands;

public class RunCommands : CoconaConsoleAppBase
{
    [Command("run", Description = "Open a file with the registered application")]
    public async Task OpenFile(
        [Argument("file", Description = "A file to be opened")] string filePath,
        [Option('e', Description = "An entry ID from the list of registered apps")] int? entry,
        [Option('f', Description = "Skip the app picker and choose the top-most option")] bool first,
        [Option('p', Description = "Show the app picker even when the default app is specified")] bool picker)
    {
        Context.Logger.LogInformation(
            "Opening {File} with entry={Entry}, default={PickDefaultApp}, and picker={Picker}...",
            filePath, entry, !first, picker);
        
        try
        {
            var appEntryDefinition = await GetAppDefinition(filePath, entry, first, picker);
            var runner = new Runner(Context.Logger);
            var result = await runner.Run(appEntryDefinition.Path, appEntryDefinition.Arguments);
        }
        catch (Exception e)
        {
            Context.Logger.LogError(e, "An error occurred while opening the file");
        }
    }

    private async Task<AppEntry> GetAppDefinition(string filePath, int? entry, bool first, bool picker)
    {
        var parser = new Parser(Context.Logger);
        var fileExtensionDefinition = await parser.GetFileExtensionDefinition(filePath);
        
        // The user has picked an entry ID to open the file with
        if (entry is not null)
            return await parser.GetAppEntryDefinition(fileExtensionDefinition, entry.Value, filePath);
        
        // The user wants to use the default app
        if (first)
            return await parser.GetFirstAppEntryDefinition(fileExtensionDefinition, filePath);
        
        // The user decides to pick an app at runtime
        if (picker)
            // TODO: open the app picker
            throw new NotImplementedException("The app picker has yet to be implemented!");

        if (string.IsNullOrWhiteSpace(fileExtensionDefinition.DefaultApp))
        {
            // TODO: open the app picker
            Context.Logger.LogWarning("There's no default app for the file extension and the picker is not yet implemented");
            return await parser.GetFirstAppEntryDefinition(fileExtensionDefinition, filePath);
        }
        
        // When no options are specified, use the default app
        var defaultEntry = fileExtensionDefinition.AppList.ToList().IndexOf(fileExtensionDefinition.DefaultApp);
        if (defaultEntry == -1)
            throw new ApplicationException("The default app is not in the list of registered apps!");
        
        return await parser.GetAppEntryDefinition(fileExtensionDefinition, defaultEntry, filePath);
    }
}
