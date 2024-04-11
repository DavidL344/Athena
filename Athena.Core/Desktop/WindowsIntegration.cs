using Microsoft.Extensions.Logging;

namespace Athena.Core.Desktop;

public class WindowsIntegration : IDesktopIntegration
{
    private const string Placeholder = "This class is a placeholder!";
    public bool IsRegistered { get; }

    public WindowsIntegration(ILogger<WindowsIntegration> logger)
    {
        IsRegistered = false;
        logger.LogWarning(Placeholder);
    }
    
    public void RegisterEntry()
    {
        throw new ApplicationException(Placeholder);
    }
    
    public void DeregisterEntry()
    {
        throw new ApplicationException(Placeholder);
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
        throw new ApplicationException(Placeholder);
    }
}
