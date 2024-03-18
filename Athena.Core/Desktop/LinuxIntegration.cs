using System.Reflection;
using Athena.Core.Configuration;
using Athena.Core.Desktop.Linux;
using IniParser;
using IniParser.Model;

namespace Athena.Core.Desktop;

public class LinuxIntegration : IDesktopIntegration
{
    private readonly ConfigPaths _configPaths;
    
    // mimeapps.list
    private readonly string _mimeAppsPath;
    private readonly FileIniDataParser _parser;
    
    // athena.desktop
    private readonly string _desktopFileName;
    private readonly string _desktopFilePath;
    
    // Athena.Cli <--> athena
    private readonly string _appPath;
    private readonly string _symlinkPath;

    public LinuxIntegration(string mimeAppsPath, ConfigPaths configPaths, FileIniDataParser parser)
    {
        _configPaths = configPaths;
        
        _desktopFileName = "athena.desktop";
        _desktopFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local", "share", "applications", _desktopFileName);
        
        // On Linux, the assembly's location points to its dll instead of the executable
        var assemblyLocation = Assembly.GetEntryAssembly()!.Location;
        _appPath = Path.ChangeExtension(assemblyLocation, null);
        
        _symlinkPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local", "bin", "athena");
        
        _mimeAppsPath = mimeAppsPath;
        _parser = parser;
    }
    
    public LinuxIntegration() : this(GetMimeAppsPath(), new ConfigPaths(),
        new FileIniDataParser { Parser = { Configuration = { AssigmentSpacer = "" } } }) { }
    
    public void RegisterEntry()
    {
        // Create a symlink to the executable in ~/.local/bin
        SymlinkEntry.Create(_symlinkPath, _appPath);
        
        // Add a .desktop file to ~/.local/share/applications
        DesktopEntry.Create(_desktopFilePath);
    }
    
    public void DeregisterEntry()
    {
        SymlinkEntry.Delete(_symlinkPath);
        DesktopEntry.Delete(_desktopFilePath);
    }
    
    public void AssociateWithApps()
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.AssociateWithAllApps(
            mimeApps, _desktopFileName);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void AssociateWithApp(string mimeType)
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.AssociateWithApp(
            mimeApps, mimeType, _desktopFileName);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void DissociateFromApps()
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.DissociateFromAllApps(
            mimeApps, _desktopFileName);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void DissociateFromApp(string fileExtensionOrMimeType)
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.DissociateFromApp(
            mimeApps, fileExtensionOrMimeType, _desktopFileName);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void SetAsDefault()
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.SetAsDefaultForAllMimeTypes(
            mimeApps, _desktopFileName);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void SetAsDefaultFor(string fileExtensionOrMimeType)
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.SetAsDefaultForMimeType(
            mimeApps, fileExtensionOrMimeType, _desktopFileName);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void UnsetDefault()
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.UnsetDefault(
            mimeApps, _desktopFileName);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void UnsetDefaultFrom(string fileExtensionOrMimeType)
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.UnsetDefaultFrom(
            mimeApps, fileExtensionOrMimeType, _desktopFileName);
        
        WriteMimeApps(modifiedMimeApps);
    }

    public void BackupAllEntries(string backupPath)
    {
        var mimeApps = ReadMimeApps();
        WriteMimeApps(mimeApps, backupPath);
    }
    
    public void RestoreAllEntries(string backupPath)
    {
        var mimeApps = _parser.ReadFile(backupPath);
        WriteMimeApps(mimeApps);
    }

    public string ConsoleStatus()
    {
        var status = new LinuxStatus
        {
            AppPath = _appPath,
            SymlinkPath = _symlinkPath,
            DesktopFilePath = _desktopFilePath,
            ConfigDir = _configPaths.Root
        };
        
        return status.ToSpectreConsole();
    }
    
    private IniData ReadMimeApps()
    {
        if (!File.Exists(_mimeAppsPath))
            throw new FileNotFoundException("The local MIME apps file doesn't exist!", _mimeAppsPath);
        
        return _parser.ReadFile(_mimeAppsPath);
    }
    
    private void WriteMimeApps(IniData mimeApps)
    {
        _parser.WriteFile(_mimeAppsPath, mimeApps);
    }
    
    private void WriteMimeApps(IniData mimeApps, string filePath)
    {
        _parser.WriteFile(filePath, mimeApps);
    }

    private static string GetMimeAppsPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mimeapps.list");
    }
}
