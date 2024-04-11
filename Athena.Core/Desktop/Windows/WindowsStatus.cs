namespace Athena.Core.Desktop.Windows;

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

    public bool IsRegistered
        => IsInPath;

    public string ToSpectreConsole()
    {
        const string registrationCommand = "athena integration --add";
        var registrationStatus = $"[red]\u25cf Not registered (run `{registrationCommand}` to register)[/]";
        
        if (IsInPath)
            registrationStatus = $"[green]\u25cf Registered[/] at [link=file://{AppPathDir}]{AppPathDir}[/]\n\t" +
                                 $"[blue]Running instance: [link=file://{AppPathDir}]{AppPath}[/][/]";

        return $"{registrationStatus}" +
               $"\n\t[blue]Config directory: [link=file://{ConfigDir}]{ConfigDir}[/][/]";
    }
}
