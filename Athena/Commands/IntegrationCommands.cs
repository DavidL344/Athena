using System.Reflection;
using Athena.Commands.Internal;
using Athena.CoreOld;
using Cocona;
using Spectre.Console;

namespace Athena.Commands;

public class IntegrationCommands : ICommands
{
    private readonly Runner _runner;
    private readonly string _symlinkPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin", "athena");
    
    public IntegrationCommands(Runner runner)
    {
        _runner = runner;
    }
    
    [Command("status", Description = "Show the current status of Athena")]
    public void Status()
    {
        var registrationStatus = File.Exists(_symlinkPath)
            ? $"[green]\u25cf Registered[/] at {_symlinkPath}"
            : "[red]\u25cf Not registered (run `athena integration --add` to register)[/]";
        
        AnsiConsole.MarkupLine($"{registrationStatus}");
    }
    
    [Command("integration", Description = "Integrate Athena into your system")]
    public async Task Integration(
        [Option('a', Description = "Add the integration")] bool add,
        [Option('r', Description = "Remove the integration")] bool remove)
    {
        if (add && !File.Exists(_symlinkPath))
        {
            // On Linux, the assembly's location points to its dll instead of the executable
            var appPath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, null);
            await _runner.Run("ln", $"-s {appPath} {_symlinkPath}");
            return;
        }

        if (remove && File.Exists(_symlinkPath))
        {
            File.Delete(_symlinkPath);
            return;
        }
        
        Status();
    }
}
