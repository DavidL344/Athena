using System.Text;
using Athena.Core.Model.AppPicker;
using Spectre.Console;

namespace Athena.Cli.Terminal;

public class AppPicker : IAppPicker
{
    public static async Task<int> Show(AppPickerContext ctx)
    {
        var appList = ctx.AppEntries;
        
        var appListReturnCode = new[] { -1, 0 };
        if (appList.Length < 2)
            return appListReturnCode[appList.Length];
        
        var promptText = $"Select an app to open [green]{ctx.FriendlyName}[/] with:";
        int index;
        
        try
        {
            var selection = new SelectionPrompt<string>()
                .Title(promptText)
                .PageSize(6)
                .MoreChoicesText("[grey](Move up and down to reveal more entries)[/]")
                .AddChoices(appList);
            selection.AddChoice("[red]Cancel[/]");
            
            var prompt = AnsiConsole.Prompt(selection);
            index = Array.IndexOf(appList, prompt);
        }
        catch (NotSupportedException)
        {
            // Fall back to Console.Read() if the terminal doesn't support the required interactivity
            var fallbackEntries = new StringBuilder();
            for (var i = 0; i < appList.Length; i++)
                fallbackEntries.Append($"\n{i}) {appList[i]}");
            
            Console.Write($"Available entries: {fallbackEntries}\n\nPlease select an entry: ");
            var readIndex = Console.ReadLine();
            await Console.Out.FlushAsync();
            
            if (!int.TryParse(readIndex, out index))
                return -1;
        }
        
        return index > appList.Length - 1 ? -1 : index;
    }
}
