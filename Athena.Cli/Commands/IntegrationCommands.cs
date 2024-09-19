using Athena.Cli.Commands.Internal;
using Athena.Desktop.System;
using Cocona;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Athena.Cli.Commands;

public class IntegrationCommands : ICommands
{
    private readonly IDesktopIntegration _desktopIntegration;
    private readonly ILogger<IntegrationCommands> _logger;

    public IntegrationCommands(IDesktopIntegration desktopIntegration, ILogger<IntegrationCommands> logger)
    {
        _desktopIntegration = desktopIntegration;
        _logger = logger;
    }
    
    [Command("status", Description = "Show the current status of Athena")]
    public void Status()
    {
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
            _desktopIntegration.RegisterEntry();
            
            string[] fileExtensionsOrMimeTypes = OperatingSystem.IsWindows()
                ? [/*".txt", ".mp3", ".mp4", "http:", "https:"*/]
                : ["text/plain", "audio/mp3", "video/mp4", "x-scheme-handler/http", "x-scheme-handler/https"];
            
            _desktopIntegration.AssociateWithApps(fileExtensionsOrMimeTypes);
            
            if (OperatingSystem.IsWindows())
                _logger.LogWarning(
                    "Please restart your terminal to finish registering Athena!");
            
            return 0;
        }
        
        if (remove)
        {
            _desktopIntegration.DeregisterEntry();
            
            if (OperatingSystem.IsWindows())
                _logger.LogWarning("Please restart your terminal to finish the removal process!");
            
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
    
    [Command("register", Description = "Register files or URLs with the shell")]
    public void Register(
        [Argument(Name = "File/URL type",
            Description = "The file extension (Windows) / MIME type (Linux) or URL to register")]
        IEnumerable<string> fileExtMimeUrls,
        [Option('r', Description = "Deregister the specified arguments instead")]
        bool remove)
    {
        if (!_desktopIntegration.IsRegistered)
            throw new ApplicationException("Athena is not registered with the system!");
        
        if (remove)
        {
            _desktopIntegration.DissociateFromApps(fileExtMimeUrls);
            return;
        }
        
        _desktopIntegration.AssociateWithApps(fileExtMimeUrls);
    }
}
