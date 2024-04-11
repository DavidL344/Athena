using System.Reflection;
using Athena.Core.Configuration;
using Athena.Core.Desktop.Windows;

namespace Athena.Core.Desktop;

public class WindowsIntegration : IDesktopIntegration
{
    // Placeholder for yet-to-be-implemented methods
    private const string Placeholder = "This class is a placeholder!";
    
    // %PATH%
    private readonly string _appPathDir;
    
    // Integration status
    public bool IsRegistered { get; }
    private readonly WindowsStatus _status;
    
    public WindowsIntegration(ConfigPaths configPaths)
    {
        var assemblyLocation = Assembly.GetEntryAssembly()!.Location;
        var appPath = Path.ChangeExtension(assemblyLocation, "exe");
        _appPathDir = Path.GetDirectoryName(appPath)!;

        _status = new WindowsStatus
        {
            AppPath = appPath,
            ConfigDir = configPaths.Root
        };
        
        IsRegistered = _status.IsRegistered;
    }
    
    public void RegisterEntry()
    {
        PathEntry.Add(_appPathDir);
    }
    
    public void DeregisterEntry()
    {
        PathEntry.Remove(_appPathDir);
    }
    
    public void AssociateWithApps(IEnumerable<string> fileExtensions)
    {
        throw new ApplicationException(Placeholder);
    }
    
    public void AssociateWithApp(string fileExtension, bool source = true)
    {
        throw new ApplicationException(Placeholder);
    }
    
    public void DissociateFromApps(IEnumerable<string> fileExtensions)
    {
        throw new ApplicationException(Placeholder);
    }
    
    public void DissociateFromApp(string fileExtension, bool source = true)
    {
        throw new ApplicationException(Placeholder);
    }
    
    public string ConsoleStatus()
    {
        return _status.ToSpectreConsole();
    }
}
