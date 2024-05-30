using Athena.Cli.Commands.Internal;
using Athena.Core.Desktop;
using Cocona;
using Spectre.Console;

namespace Athena.Cli.Commands;

public class IntegrationCommands : ICommands
{
    private readonly IDesktopIntegration _desktopIntegration;

    public IntegrationCommands(IDesktopIntegration desktopIntegration)
    {
        _desktopIntegration = desktopIntegration;
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
            
            if (!OperatingSystem.IsWindows())
                _desktopIntegration.AssociateWithApps(fileExtensionsOrMimeTypes);
            
            return 0;
        }
        
        if (remove)
        {
            _desktopIntegration.DeregisterEntry();
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
