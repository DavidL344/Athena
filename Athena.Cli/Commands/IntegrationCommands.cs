using System.Reflection;
using Athena.Cli.Commands.Internal;
using Athena.Core.Configuration;
using Cocona;
using Spectre.Console;

namespace Athena.Cli.Commands;

public class IntegrationCommands : ICommands
{
    private readonly ConfigPaths _configPaths;
    private readonly string _appPath;
    private readonly string _appPathDir;
    private readonly string _symlinkPath;

    public IntegrationCommands(ConfigPaths configPaths)
    {
        _configPaths = configPaths;
        
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
        var registrationCommand = OperatingSystem.IsWindows()
            ? "athena integration --add"
            : "./Athena integration --add";
        var registrationStatus = $"[red]\u25cf Not registered (run `{registrationCommand}` to register)[/]";
        
        if (OperatingSystem.IsWindows())
        {
            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            var pattern = path!.EndsWith(';') ? $"{_appPathDir};" : $";{_appPathDir}";
            
            if (path.Contains(pattern))
                registrationStatus = $"[green]\u25cf Registered[/] at {_appPath}" +
                                     $"[blue]Running instance: {_appPath}[/]";
        }
        
        if (OperatingSystem.IsLinux() && File.Exists(_symlinkPath))
        {
            var symlinkTarget = File.ResolveLinkTarget(_symlinkPath, true)!.FullName;
            var symlinkStyle = File.Exists(symlinkTarget) ? "darkorange" : "red";
            var targetFound = !File.Exists(symlinkTarget) ? " but not found" : "";
            
            var symlinkDir = $"file://{Path.GetDirectoryName(_symlinkPath)}";
            var symlinkTargetDir = $"file://{Path.GetDirectoryName(symlinkTarget)}";
            var appDir = $"file://{Path.GetDirectoryName(_appPath)}";
            
            registrationStatus = symlinkTarget == _appPath
                ? $"[green]\u25cf Registered[/] at [link={symlinkDir}]{_symlinkPath}[/]"
                : $"[{symlinkStyle}]\u25cf Another instance registered{targetFound}[/] at [link={symlinkDir}]{_symlinkPath}[/]" +
                  $"\n\t[green]Running instance: [link={appDir}]{_appPath}[/][/]" +
                  $"\n\t[{symlinkStyle}]Symlink instance: [link={symlinkTargetDir}]{symlinkTarget}[/][/]";
        }
        
        AnsiConsole.MarkupLine($"{registrationStatus}" +
                               $"\n\t[blue]Config directory: [link=file://{_configPaths.Root}]{_configPaths.Root}[/][/]");
    }
    
    [Command("integration", Description = "Integrate Athena into your system")]
    public void Integration(
        [Option('a', Description = "Add the integration")] bool add,
        [Option('r', Description = "Remove the integration")] bool remove,
        [Option('s', Description = "Display the integration status")] bool status)
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
            File.CreateSymbolicLink(_symlinkPath, _appPath);
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

        if (status)
        {
            Status();
            return;
        }
        
        AnsiConsole.WriteLine("Error: Option 'action' is required. See '--help' for usage.");
    }
}
