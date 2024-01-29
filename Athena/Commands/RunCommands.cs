using System.Text.Json;
using Athena.Internal;
using Athena.Model;
using CliWrap;
using CliWrap.Buffered;
using Cocona;
using Microsoft.Extensions.Logging;

namespace Athena.Commands;

public class RunCommands : CoconaConsoleAppBase
{
    [Command("run", Description = "Open a file with the registered application")]
    public async Task OpenFile(
        [Argument(Description = "A file to be opened")] string file,
        [Option('e', Description = "An entry ID from the list of registered apps")] int? entry,
        [Option('f', Description = "Skip the app picker and choose the top-most option")] bool first,
        [Option('p', Description = "Show the app picker even when the default app is specified")] bool picker)
    {
        Context.Logger.LogInformation(
            "Opening {File} with entry={Entry}, default={PickFirstApp}, and picker={Picker}...",
            file, entry, !first, picker);
        
        if (Path.GetExtension(file).Length == 0)
            throw new ApplicationException("The file has no extension!");
        
        var fileExtension = Path.GetExtension(file).Substring(1);
        var definitionPath = Path.Combine(Vars.AppDataDir, "files", $"{fileExtension}.json");

        if (!File.Exists(definitionPath))
            throw new ApplicationException($"The file extension ({fileExtension}) isn't registered with Athena!");
        
        var definitionData = await File.ReadAllTextAsync(definitionPath);
        var definition = JsonSerializer.Deserialize<FileExtension>(definitionData, Vars.JsonSerializerOptions);

        if (definition is null)
            throw new ApplicationException("The file extension definition is invalid!");

        if (definition.AppList.Length == 0)
            throw new ApplicationException("The file extension has no associated entries!");
        
        // The user has picked an entry ID to open the file with
        if (entry is not null)
        {
            if (entry < 0 || entry >= definition.AppList.Length)
                throw new ApplicationException("The entry ID is out of range!");
            
            await OpenFileWithEntry(file, definition, entry.Value);
            return;
        }
        
        // The user wants to use the default app
        if (first)
        {
            Context.Logger.LogInformation("The first entry is {FirstEntry}", definition.AppList[0]);
            await OpenFileWithEntry(file, definition, 0);
            return;
        }
        
        // The user decides to pick an app at runtime
        if (picker)
        {
            throw new NotImplementedException("The app picker has yet to be implemented!");
        }

        if (string.IsNullOrWhiteSpace(definition.DefaultApp))
        {
            Context.Logger.LogInformation("There's no default app for the file extension and the picker is not yet implemented");
            Context.Logger.LogInformation("The first entry is {FirstEntry}", definition.AppList[0]);
            await OpenFileWithEntry(file, definition, 0);
            return;
        }
        
        var defaultEntry = definition.AppList.ToList().IndexOf(definition.DefaultApp);
        if (defaultEntry == -1)
            throw new ApplicationException("The default app is not in the list of registered apps!");
        
        await OpenFileWithEntry(file, definition, defaultEntry);
    }

    private async Task OpenFileWithEntry(string file, FileExtension definition, int entryIndex)
    {
        var entry = definition.AppList[entryIndex];
        var appEntry = Path.Combine(Vars.AppDataDir, "entries", $"{entry}.json");
        
        if (!File.Exists(appEntry))
            throw new ApplicationException($"The entry ({entry}) isn't defined!");
        
        var appEntryData = await File.ReadAllTextAsync(appEntry);
        var appEntryDefinition = JsonSerializer.Deserialize<AppEntry>(appEntryData, Vars.JsonSerializerOptions);
        
        if (appEntryDefinition is null)
            throw new ApplicationException("The entry definition is invalid!");
        
        Context.Logger.LogInformation(
            "Opening {Name} with {Path} and params {Arguments}...",
            appEntryDefinition.Name, appEntryDefinition.Path, appEntryDefinition.Arguments);

        var arguments = Environment.ExpandEnvironmentVariables(appEntryDefinition.Arguments
            .Replace("~", "%HOME%")
            .Replace("$FILE", file)
            .Replace("$URL", file));
        
        await Cli.Wrap(appEntryDefinition.Path)
            .WithArguments(arguments)
            .WithStandardInputPipe(PipeSource.FromStream(Console.OpenStandardInput()))
            .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .ExecuteBufferedAsync();
    }
}
