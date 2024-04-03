namespace Athena.Core.Desktop.Linux;

public class LinuxStatus
{
    public string SymlinkPath { get; init; } = default!;
    public string DesktopFilePath { get; init; } = default!;
    public string AppPath { get; init; } = default!;
    public string ConfigDir { get; init; } = default!;
    
    private string SymlinkTarget
        => File.ResolveLinkTarget(SymlinkPath, true)!.FullName;
    
    private bool SymlinkExists
        => File.Exists(SymlinkPath);
    
    private bool SymlinkTargetExists
        => File.Exists(SymlinkTarget);
    
    private bool IsSymlinkRegistered
        => SymlinkExists && SymlinkTargetExists && SymlinkTarget == AppPath;
    
    private bool IsDesktopFileRegistered
        => File.Exists(DesktopFilePath);
    
    private bool IsAtLeastPartiallyRegistered
        => IsSymlinkRegistered || IsDesktopFileRegistered;

    public string ToSpectreConsole()
    {
        const string registrationCommand = "./Athena integration --add";

        var registrationStatus = "[$STATUS]\u25cf $REGISTERED[/]$AT";
        var statusStyle = "";
        var statusText = "";
        var registeredAt = "";

        if (!IsAtLeastPartiallyRegistered)
        {
            statusStyle = "red";
            statusText = $"Not registered (run `{registrationCommand}` to register)";
        }
        
        if (SymlinkExists)
        {
            var symlinkDir = Path.GetDirectoryName(SymlinkPath);
            var appDir = Path.GetDirectoryName(AppPath);
            
            var targetFound = !File.Exists(SymlinkTarget) ? " but not found" : "";
            var thisInstanceIsRegistered = SymlinkTarget == AppPath;
            var symlinkStyle = thisInstanceIsRegistered ? "blue" : File.Exists(SymlinkTarget) ? "darkorange" : "red";
            
            statusStyle = thisInstanceIsRegistered ? "green" : "darkorange";
            statusText = thisInstanceIsRegistered ? "Registered" : $"Another instance registered{targetFound}";
            registeredAt = $" at [link=file://{symlinkDir}]{SymlinkPath}[/]";
            
            registrationStatus += $"\n\tRunning instance: [blue][link=file://{appDir}]{AppPath}[/][/]";
            registrationStatus += $"\n\tSymlink instance: [{symlinkStyle}][link=file://{symlinkDir}]{SymlinkTarget}[/][/]";
        }
        
        if (IsDesktopFileRegistered)
        {
            var desktopDir = Path.GetDirectoryName(DesktopFilePath);
            
            statusStyle = File.Exists(DesktopFilePath)
                ? statusStyle == "darkorange"
                    ? "darkorange"
                    : "green"
                : "darkorange";
            
            if (File.Exists(DesktopFilePath))
                registrationStatus += $"\n\tXDG Desktop file: [blue][link=file://{desktopDir}]{DesktopFilePath}[/][/]";
        }
        
        var statusResult = string.IsNullOrEmpty(statusStyle) ? "darkorange" : statusStyle;
        var registeredResult = string.IsNullOrEmpty(statusText) ? "Partially registered" : statusText;
        var registeredAtResult = string.IsNullOrEmpty(registeredAt) ? "" : registeredAt;
        
        registrationStatus = registrationStatus
                .Replace("$STATUS", statusResult)
                .Replace("$REGISTERED", registeredResult)
                .Replace("$AT", registeredAtResult);
        
        registrationStatus += $"\n\tConfig directory: [blue][link=file://{ConfigDir}]{ConfigDir}[/][/]";
        
        // Shorten the displayed links in the console
        registrationStatus = registrationStatus.Replace(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "~");
        
        // The clickable link targets need to stay expanded
        registrationStatus = registrationStatus.Replace(
            "file://~", $"file://{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}");
        
        return registrationStatus;
    }
}
