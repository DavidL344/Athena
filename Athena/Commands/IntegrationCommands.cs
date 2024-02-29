using System.Reflection;
using Athena.Commands.Internal;
using Athena.Core.Runner;
using Cocona;
using Spectre.Console;

namespace Athena.Commands;

public class IntegrationCommands : ICommands
{
    private readonly AppRunner _runner;
    private readonly string _appPath;
    private readonly string _appPathDir;
    private readonly string _symlinkPath;
    
    public IntegrationCommands(AppRunner runner)
    {
        _runner = runner;
        
        // On Linux, the assembly's location points to its dll instead of the executable
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        _appPath = Path.ChangeExtension(assemblyLocation,
            OperatingSystem.IsWindows() ? "exe" : null);
        
        _symlinkPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin", "athena");
        
        _appPathDir = Path.GetDirectoryName(_appPath)!;
    }
    
    [Command("status", Description = "Show the current status of Athena")]
    public void Status()
    {
        var registrationStatus = "[red]\u25cf Not registered (run `athena integration --add` to register)[/]";
        
        if (OperatingSystem.IsWindows())
        {
            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            var pattern = path!.EndsWith(';') ? $"{_appPathDir};" : $";{_appPathDir}";
            
            if (path.Contains(pattern))
                registrationStatus = $"[green]\u25cf Registered[/] at {_appPath}";
        }
        
        if (OperatingSystem.IsLinux() && File.Exists(_symlinkPath))
        {
            var symlinkTarget = File.ResolveLinkTarget(_symlinkPath, true)!.FullName;
            registrationStatus = symlinkTarget == _appPath
                ? $"[green]\u25cf Registered[/] at {_symlinkPath}"
                : $"[darkorange]\u25cf Registered[/] at {_symlinkPath}" +
                  $"\n\tRunning instance: [green]{_appPath}[/]" +
                  $"\n\tSymlink target: [darkorange]{symlinkTarget}[/]";
        }
        
        AnsiConsole.MarkupLine($"{registrationStatus}");
    }
    
    [Command("integration", Description = "Integrate Athena into your system")]
    public async Task Integration(
        [Option('a', Description = "Add the integration")] bool add,
        [Option('r', Description = "Remove the integration")] bool remove)
    {
        if (add)
        {
            if (OperatingSystem.IsWindows())
            {
                var pathBefore = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                var pathAfter = pathBefore!.EndsWith(';')
                    ? $"{pathBefore}{_appPathDir};"
                    : $"{pathBefore};{_appPathDir}";
                Environment.SetEnvironmentVariable("PATH", pathAfter, EnvironmentVariableTarget.User);
                return;
            }
            
            if (File.Exists(_symlinkPath)) File.Delete(_symlinkPath);
            await _runner.RunAsync("ln", $"-s {_appPath} {_symlinkPath}");
            return;
        }
        
        if (remove)
        {
            if (OperatingSystem.IsWindows())
            {
                var pathBefore = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                var pathAfter = pathBefore!.EndsWith(';')
                    ? pathBefore.Replace($"{_appPathDir};", "")
                    : pathBefore.Replace($";{_appPathDir}", "");
                Environment.SetEnvironmentVariable("PATH", pathAfter, EnvironmentVariableTarget.User);
                return;
            }
            
            if (File.Exists(_symlinkPath)) File.Delete(_symlinkPath);
            return;
        }
        
        Status();
    }
}
