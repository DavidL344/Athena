namespace Athena.Core.Desktop.Linux;

public class IntegrationPaths
{
    public required string DesktopFileDir { get; init; }
    public required string SymlinkDir { get; init; }
    public required string AppDir { get; init; }
    
    public required string DesktopFileName { get; init; }
    public required string SymlinkFileName { get; init; }
    public required string AppName { get; init; }
    
    public string DesktopFilePath => Path.Combine(DesktopFileDir, DesktopFileName);
    public string SymlinkFilePath => Path.Combine(SymlinkDir, SymlinkFileName);
    public string AppPath => Path.Combine(AppDir, AppName);
}
