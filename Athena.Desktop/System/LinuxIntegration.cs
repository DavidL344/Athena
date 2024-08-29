using System.Reflection;
using System.Text.RegularExpressions;
using Athena.Desktop.Configuration;
using Athena.Desktop.Runner;
using Athena.Desktop.System.Linux;
using IniParser;

namespace Athena.Desktop.System;

public partial class LinuxIntegration : IDesktopIntegration
{
    // XDG Desktop
    private readonly AppRunner _appRunner;
    private readonly string _desktopFileDir;
    private readonly FileIniDataParser _parser;
    
    // Athena.Cli
    private readonly IntegrationPaths _athenaCli;
    
    // Athena.Gtk
    private readonly IntegrationPaths _athenaGtk;
    
    // Symlink directory in $PATH
    private readonly string _symlinkDir;
    
    // Integration status
    public bool IsRegistered { get; }
    private readonly LinuxStatus _status;

    public LinuxIntegration(ConfigPaths configPaths,
        FileIniDataParser parser, AppRunner appRunner)
    {
        _appRunner = appRunner;
        _parser = parser;
        
        _desktopFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local", "share", "applications");
        
        _symlinkDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local", "bin");
        
        var assemblyLocation = Assembly.GetEntryAssembly()!.Location;
        var appDir = Path.GetDirectoryName(assemblyLocation)!;

        _athenaCli = new IntegrationPaths
        {
            DesktopFileDir = _desktopFileDir,
            SymlinkDir = _symlinkDir,
            AppDir = appDir,
            DesktopFileName = "athena.desktop",
            SymlinkFileName = "athena",
            AppName = "Athena"
        };
        
        _athenaGtk = new IntegrationPaths
        {
            DesktopFileDir = _desktopFileDir,
            SymlinkDir = _symlinkDir,
            AppDir = appDir
#if DEBUG
                .Replace("Athena.Cli", "Athena.Gtk")
#endif
            ,
            DesktopFileName = "athena-gtk.desktop",
            SymlinkFileName = "athena-gtk",
            AppName = "Athena.Gtk"
        };
        
        _status = new LinuxStatus
        {
            AppPath = _athenaCli.AppPath,
            SymlinkPath = _athenaCli.SymlinkFilePath,
            DesktopFilePath = _athenaCli.DesktopFilePath,
            ConfigDir = configPaths.Root
        };
        
        IsRegistered = _status.IsRegistered;
    }
    
    public LinuxIntegration(ConfigPaths configPaths, AppRunner appRunner) :
        this(configPaths, GetParserSettings(), appRunner) { }
    
    public void RegisterEntry()
    {
        // Create a directory for symlinks if it doesn't exist
        if (!Directory.Exists(_symlinkDir))
            Directory.CreateDirectory(_symlinkDir);
        
        // Enable shell completion support
        BashRcEntry.Add(_symlinkDir);
        
        // Create symlinks to the executables in ~/.local/bin
        SymlinkEntry.Create(_athenaCli.SymlinkFilePath, _athenaCli.AppPath);
        SymlinkEntry.Create(_athenaGtk.SymlinkFilePath, _athenaGtk.AppPath);

        // Add XDG Desktop files to ~/.local/share/applications
        DesktopEntry.Add(_athenaCli.DesktopFilePath);
        DesktopEntry.Add(_athenaGtk.DesktopFilePath);
        
        // Update the shell
        DesktopEntry.Source(_desktopFileDir, _appRunner);
    }
    
    public void DeregisterEntry()
    {
        // Disable shell completion support
        BashRcEntry.Remove(_symlinkDir);
        
        // Remove symlinks
        SymlinkEntry.Delete(_athenaCli.SymlinkFilePath);
        SymlinkEntry.Delete(_athenaGtk.SymlinkFilePath);
        
        // Remove XDG Desktop files
        DesktopEntry.Remove(_athenaCli.DesktopFilePath);
        DesktopEntry.Remove(_athenaGtk.DesktopFilePath);
        
        // Update the shell
        DesktopEntry.Source(_desktopFileDir, _appRunner);
        
        // Remove the directory for symlinks if it's empty
        if (!Directory.Exists(_symlinkDir)) return;
        if (Directory.GetFiles(_symlinkDir).Length != 0 || Directory.GetDirectories(_symlinkDir).Length != 0) return;
        Directory.Delete(_symlinkDir);
    }

    public void AssociateWithApps(IEnumerable<string> mimeTypes)
    {
        foreach (var mimeType in mimeTypes)
        {
            AssociateWithApp(mimeType, false);
        }
        DesktopEntry.Source(_desktopFileDir, _appRunner);
    }
    
    public void AssociateWithApp(string mimeType, bool source = true)
    {
        var pattern = MimeTypeRegex();
        if (!pattern.IsMatch(mimeType))
            throw new ArgumentException("The specified MIME type is invalid!", nameof(mimeType));
        
        DesktopEntry.AddMimeType(_athenaGtk.DesktopFilePath, mimeType, _parser);
        if (source) DesktopEntry.Source(_desktopFileDir, _appRunner);
    }
    
    public void DissociateFromApps(IEnumerable<string> mimeTypes)
    {
        foreach (var mimeType in mimeTypes)
        {
            DissociateFromApp(mimeType, false);
        }
        DesktopEntry.Source(_desktopFileDir, _appRunner);
    }
    
    public void DissociateFromApp(string mimeType, bool source = true)
    {
        var pattern = MimeTypeRegex();
        if (!pattern.IsMatch(mimeType))
            throw new ArgumentException("The specified MIME type is invalid!", nameof(mimeType));
        
        DesktopEntry.RemoveMimeType(_athenaGtk.DesktopFilePath, mimeType, _parser);
        DesktopEntry.Source(_desktopFileDir, _appRunner);
    }
    
    public string ConsoleStatus()
    {
        return _status.ToSpectreConsole();
    }
    
    private static FileIniDataParser GetParserSettings()
    {
        return new FileIniDataParser { Parser = { Configuration = { AssigmentSpacer = "" } } };
    }

    [GeneratedRegex(@"^[a-zA-Z0-9\-]+\/[a-zA-Z0-9\-\.\+]+$")]
    private static partial Regex MimeTypeRegex();
}
