using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Athena.Core.Desktop.Windows;

[SupportedOSPlatform("windows")]
public class WindowsStatus
{
    public string AppPath { get; init; } = default!;
    public string ConfigDir { get; init; } = default!;
    
    private string AppPathDir
        => Path.GetDirectoryName(AppPath)!;

    private bool IsInPath
    {
        get
        {
            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            var pattern = path!.EndsWith(';') ? $"{AppPathDir};" : $";{AppPathDir}";

            return path.Contains(pattern);
        }
    }

    private bool IsInContextMenu
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Classes\*\shell\OpenWithAthena\command", false);
            
            return key is not null && key.GetValue(null) as string == $"\"{AppPath}\" run \"%1\"";
        }
    }

    public bool IsRegistered
        => IsInPath;

    public string ToSpectreConsole()
    {
        const string registrationCommand = "athena integration --add";
        var registrationStatus = $"[red]\u25cf Not registered (run `{registrationCommand}` to register)[/]";
        
        if (IsInPath)
            registrationStatus = $"[green]\u25cf Registered[/] at [link=file://{AppPathDir}]{AppPathDir}[/]";
        
        registrationStatus += $"\n\tRunning instance: [blue][link=file://{AppPathDir}]{AppPath}[/][/]" +
                              $"\n\tConfig directory: [blue][link=file://{ConfigDir}]{ConfigDir}[/][/]";
        
        var entryStatus = IsInContextMenu ? "[green]Active[/]" : "[red]Missing[/]";
        registrationStatus += $"\n\tContext menu entry: {entryStatus}";
        
        return registrationStatus;

    }
}
