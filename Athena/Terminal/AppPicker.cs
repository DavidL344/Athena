using Athena.Core.Model.Opener;
using Spectre.Console;

namespace Athena.Terminal;

public class AppPicker
{
    public static int Show(IOpener opener)
    {
        if (opener.AppList.Length == 1) return 0;
        
        var selection = new SelectionPrompt<string>()
            .Title("Select an app to open the file with")
            .PageSize(10)
            .AddChoices(opener.AppList);
        selection.AddChoice("Cancel");

        var prompt = AnsiConsole.Prompt(selection);
        var index = Array.IndexOf(opener.AppList, prompt);
        
        return index > opener.AppList.Length - 1 ? -1 : index;
    }
}
