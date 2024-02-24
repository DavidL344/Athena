using System.Reflection;
using System.Runtime.InteropServices;
using Athena.Commands.Internal;
using Athena.Core.Runner;
using Cocona;
using Spectre.Console;

namespace Athena.Commands;

public class IntegrationCommands : ICommands
{
    private readonly AppRunner _runner;
    private readonly string _appPath;
    private readonly string _symlinkPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin", "athena");
    
    public IntegrationCommands(AppRunner runner)
    {
        _runner = runner;
        
        // On Linux, the assembly's location points to its dll instead of the executable
        _appPath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, null);
    }
    
    [Command("status", Description = "Show the current status of Athena")]
    public void Status()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            AnsiConsole.MarkupLine("[red]\u25cf This feature isn't supported on Windows![/]");
            return;
        }

        string registrationStatus;
        
        if (File.Exists(_symlinkPath))
        {
            var symlinkTarget = File.ResolveLinkTarget(_symlinkPath, true)!.FullName;
            registrationStatus = symlinkTarget == _appPath
                ? $"[green]\u25cf Registered[/] at {_symlinkPath} --> [green]{_appPath}[/]"
                : $"[darkorange]\u25cf Registered[/] at {_symlinkPath}" +
                  $"\n\tRunning instance: [green]{_appPath}[/]" +
                  $"\n\tSymlink target: [darkorange]{symlinkTarget}[/]";
        }
        else
        {
            registrationStatus = "[red]\u25cf Not registered (run `athena integration --add` to register)[/]";
        }
        
        AnsiConsole.MarkupLine($"{registrationStatus}");
    }
    
    [Command("integration", Description = "Integrate Athena into your system")]
    public async Task Integration(
        [Option('a', Description = "Add the integration")] bool add,
        [Option('r', Description = "Remove the integration")] bool remove)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            AnsiConsole.MarkupLine("[red]\u25cf This feature isn't supported on Windows![/]");
            return;
        }
        
        if (add)
        {
            if (File.Exists(_symlinkPath)) File.Delete(_symlinkPath);
            await _runner.RunAsync("ln", $"-s {_appPath} {_symlinkPath}");
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
