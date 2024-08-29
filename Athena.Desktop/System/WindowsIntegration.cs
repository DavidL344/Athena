using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Athena.Desktop.Configuration;
using Athena.Desktop.System.Windows;
using Microsoft.Extensions.Logging;

namespace Athena.Desktop.System;

[SupportedOSPlatform("windows")]
public partial class WindowsIntegration : IDesktopIntegration
{
    // %PATH%
    private readonly string _appPath;
    private readonly string _appPathDir;
    
    // Integration status
    public bool IsRegistered { get; }
    private readonly WindowsStatus _status;
    
    // Logger
    private readonly ILogger<WindowsIntegration> _logger;
    
    public WindowsIntegration(ConfigPaths configPaths, ILogger<WindowsIntegration> logger)
    {
        _logger = logger;
        
        var assembly = Assembly.GetEntryAssembly()!;
        
        // The assembly's location points to its dll instead of the executable
        _appPath = Path.ChangeExtension(assembly.Location, "exe");
        _appPathDir = Path.GetDirectoryName(_appPath)!;

        _status = new WindowsStatus
        {
            AppPath = _appPath,
            ConfigDir = configPaths.Root
        };
        
        IsRegistered = _status.IsRegistered;
    }
    
    public void RegisterEntry()
    {
        PathEntry.Add(_appPathDir);
        AssociateWithApp("*");
    }
    
    public void DeregisterEntry()
    {
        DissociateFromApp("*");
        PathEntry.Remove(_appPathDir);
    }
    
    public void AssociateWithApps(IEnumerable<string> fileExtensions)
    {
        foreach (var fileExtension in fileExtensions)
        {
            AssociateWithApp(fileExtension, false);
        }
        RegistryEntry.Source();
    }
    
    public void AssociateWithApp(string fileExtension, bool source = true)
    {
        var fileExtensionPattern = FileExtensionRegex();
        if (fileExtensionPattern.IsMatch(fileExtension))
        {
            if (fileExtension != "*")
                _logger.LogWarning(
                    "This feature is currently unstable: already associated file extensions might not be updated (please use \"*\" as a workaround)!");
            
            RegistryEntry.AddToContextMenu(
                RegistryEntry.Type.FileExtension, fileExtension.ToLower(),
                _appPath.Replace("Athena.exe", "Athena.Wpf.exe")
#if DEBUG
                    .Replace("Athena.Cli", "Athena.Wpf")
#endif
                    , "%1");
            
            if (source) RegistryEntry.Source();
            return;
        }
        
        var protocolPattern = ProtocolRegex();
        if (protocolPattern.IsMatch(fileExtension))
        {
            var protocol = fileExtension
                .Replace(":", string.Empty)
                .Replace("/", string.Empty)
                .ToLower();
            
            RegistryEntry.AddToContextMenu(RegistryEntry.Type.Protocol, protocol, _appPath, "run %1");
            
            if (source) RegistryEntry.Source();
            return;
        }
        
        throw new ArgumentException(
            "The specified file extension or protocol is invalid!",
            nameof(fileExtension));
    }
    
    public void DissociateFromApps(IEnumerable<string> fileExtensions)
    {
        foreach (var fileExtension in fileExtensions)
        {
            DissociateFromApp(fileExtension, false);
        }
        RegistryEntry.Source();
    }
    
    public void DissociateFromApp(string fileExtension, bool source = true)
    {
        var fileExtensionPattern = FileExtensionRegex();
        if (fileExtensionPattern.IsMatch(fileExtension))
        {
            RegistryEntry.RemoveFromContextMenu(RegistryEntry.Type.FileExtension, fileExtension.ToLower());
            
            if (source) RegistryEntry.Source();
            return;
        }
        
        var protocolPattern = ProtocolRegex();
        if (protocolPattern.IsMatch(fileExtension))
        {
            var protocol = fileExtension
                .Replace(":", string.Empty)
                .Replace("/", string.Empty)
                .ToLower();
            
            RegistryEntry.RemoveFromContextMenu(RegistryEntry.Type.Protocol, protocol);
            
            if (source) RegistryEntry.Source();
            return;
        }
        
        throw new ArgumentException(
            "The specified file extension or protocol is invalid!",
            nameof(fileExtension));
    }
    
    public string ConsoleStatus()
    {
        return _status.ToSpectreConsole();
    }
    
    [GeneratedRegex(@"^(\*)|(\.[a-zA-Z0-9_\-\.+]+)$")]
    private static partial Regex FileExtensionRegex();
    
    [GeneratedRegex(@"^[a-zA-Z]+:[\/]{0,2}$")]
    private static partial Regex ProtocolRegex();
}
