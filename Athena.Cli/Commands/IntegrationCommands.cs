using System.Reflection;
using Athena.Cli.Commands.Internal;
using Athena.Core.Configuration;
using Athena.Core.Desktop;
using Cocona;
using Spectre.Console;

namespace Athena.Cli.Commands;

public class IntegrationCommands : ICommands
{
    private readonly ConfigPaths _configPaths;
    private readonly string _appPath;
    private readonly string _appPathDir;
    private readonly string _backupDir;
    private readonly IDesktopIntegration _desktopIntegration;
    private readonly string _currentDate;

    public IntegrationCommands(ConfigPaths configPaths, IDesktopIntegration desktopIntegration)
    {
        _backupDir = Path.Combine(configPaths.Root, "backup");
        _configPaths = configPaths;
        _desktopIntegration = desktopIntegration;
        _currentDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        
        // On Linux, the assembly's location points to its dll instead of the executable
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        _appPath = Path.ChangeExtension(assemblyLocation,
            OperatingSystem.IsWindows() ? "exe" : null);
        
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
            
            AnsiConsole.MarkupLine($"{registrationStatus}" +
                                   $"\n\t[blue]Config directory: [link=file://{_configPaths.Root}]{_configPaths.Root}[/][/]");
            
            return;
        }
        
        AnsiConsole.MarkupLine(_desktopIntegration.ConsoleStatus());
    }
    
    [Command("integration", Description = "Integrate Athena into your system")]
    public int Integration(
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
                return 0;
            }
            
            if (!Directory.Exists(_backupDir))
                Directory.CreateDirectory(_backupDir);

            _desktopIntegration.RegisterEntry();
            _desktopIntegration.BackupAllEntries(_backupDir, _currentDate);
            _desktopIntegration.AssociateWithApps();
            return 0;
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
                return 0;
            }
            
            _desktopIntegration.DeregisterEntry();
            _desktopIntegration.BackupAllEntries(_backupDir, _currentDate);
            _desktopIntegration.DissociateFromApps();
            return 0;
        }

        if (status)
        {
            Status();
            return 0;
        }
        
        AnsiConsole.WriteLine("Error: Option 'action' is required. See '--help' for usage.");
        return 1;
    }
}
