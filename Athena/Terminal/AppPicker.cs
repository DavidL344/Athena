using System.Text;
using Athena.Core.Model.Internal;
using Athena.Core.Parser;
using Spectre.Console;

namespace Athena.Terminal;

public class AppPicker
{
    public static async Task<int> Show(IOpener opener, AppParser appParser, bool forceShow)
    {
        var appListLength = new[] { -1, 0 };
        if (opener.AppList.Count < 2 && !forceShow)
            return appListLength[opener.AppList.Count];
        
        // Parse the names of the app entries
        var appEntries = new string[opener.AppList.Count];
        for (var i = 0; i < opener.AppList.Count; i++)
        {
            var entryDefinition = await appParser.GetAppDefinition(opener.AppList[i]);
            appEntries[i] = entryDefinition.Name;
        }
        
        var promptText = $"Select an app to open [green]{opener.Name}[/] with:";
        int index;
        
        try
        {
            var selection = new SelectionPrompt<string>()
                .Title(promptText)
                .PageSize(6)
                .MoreChoicesText("[grey](Move up and down to reveal more entries)[/]")
                .AddChoices(appEntries);
            selection.AddChoice("[red]Cancel[/]");

            var prompt = AnsiConsole.Prompt(selection);
            index = Array.IndexOf(appEntries, prompt);
        }
        catch (NotSupportedException)
        {
            // Fall back to Console.Read() if the terminal doesn't support the required interactivity
            var fallbackEntries = new StringBuilder();
            for (var i = 0; i < opener.AppList.Count; i++)
                fallbackEntries.Append($"\n{i}) {opener.AppList[i]}");

            Console.Write($"Available entries: {fallbackEntries}\n\nPlease select an entry: ");
            var readIndex = Console.ReadLine();
            await Console.Out.FlushAsync();
            
            if (!int.TryParse(readIndex, out index))
                return -1;
        }
        
        return index > opener.AppList.Count - 1 ? -1 : index;
    }
}
