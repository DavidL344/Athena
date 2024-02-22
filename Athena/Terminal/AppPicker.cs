using System.Text;
using Athena.CoreOld.Model.Opener;
using Spectre.Console;

namespace Athena.Terminal;

public class AppPicker
{
    public static int Show(IOpener opener)
    {
        var appListLength = new[] { -1, 0 };
        if (opener.AppList.Length < 2)
            return appListLength[opener.AppList.Length];

        const string promptText = "Select an app to open the file with";
        int index;
        
        try
        {
            var selection = new SelectionPrompt<string>()
                .Title(promptText)
                .PageSize(10)
                .AddChoices(opener.AppList);
            selection.AddChoice("Cancel");

            var prompt = AnsiConsole.Prompt(selection);
            index = Array.IndexOf(opener.AppList, prompt);
        }
        catch (NotSupportedException)
        {
            // Fall back to Console.Read() if the terminal doesn't support the required interactivity
            var fallbackEntries = new StringBuilder();
            for (var i = 0; i < opener.AppList.Length; i++)
                fallbackEntries.Append($"\n{i}) {opener.AppList[i]}");

            Console.Write($"Available entries: {fallbackEntries}\n\nPlease select an entry: ");
            var readIndex = Console.ReadLine();
            Console.Out.Flush();
            
            if (!int.TryParse(readIndex, out index))
                return -1;
        }
        
        return index > opener.AppList.Length - 1 ? -1 : index;
    }
}
