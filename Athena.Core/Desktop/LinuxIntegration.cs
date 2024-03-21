using System.Reflection;
using System.Text;
using Athena.Core.Configuration;
using Athena.Core.Desktop.Linux;
using Athena.Core.Desktop.Shared;
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
    private readonly string _symlinkDir;
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

        _symlinkDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local", "bin");
        _symlinkPath = Path.Combine(_symlinkDir, "athena");
        
        _mimeAppsPath = mimeAppsPath;
        _parser = parser;
    }
    
    public LinuxIntegration() : this(GetMimeAppsPath(), new ConfigPaths(),
        new FileIniDataParser { Parser = { Configuration = { AssigmentSpacer = "" } } }) { }
    
    public void RegisterEntry()
    {
        // Create a directory for symlinks if it doesn't exist
        if (!Directory.Exists(_symlinkDir))
            Directory.CreateDirectory(_symlinkDir);
        
        // Add the symlink directory to $PATH
        PathEntry.Add(_symlinkDir);
        
        // Create a symlink to the executable in ~/.local/bin
        SymlinkEntry.Create(_symlinkPath, _appPath);
        
        // Add a .desktop file to ~/.local/share/applications
        DesktopEntry.Create(_desktopFilePath);
    }
    
    public void DeregisterEntry()
    {
        SymlinkEntry.Delete(_symlinkPath);
        DesktopEntry.Delete(_desktopFilePath);
        
        // Only remove the symlink directory if it's empty
        if (!Directory.Exists(_symlinkDir)) return;
        if (Directory.GetFiles(_symlinkDir).Length != 0 || Directory.GetDirectories(_symlinkDir).Length != 0) return;
        
        PathEntry.Remove(_symlinkDir);
        PathEntry.Remove(_symlinkDir);
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

    public void BackupAllEntries(string backupDir, string identifier)
    {
        var mimeApps = ReadMimeApps();
        WriteMimeApps(mimeApps, Path.Combine(backupDir, $"mimeapps.list.{identifier}.bak"));

        var pathBackup = Path.Combine(backupDir, $"path.{identifier}.bak");
        File.WriteAllText(pathBackup, PathEntry.Get());
    }
    
    public void RestoreAllEntries(string backupDir, string identifier, string newBackupIdentifier)
    {
        var mimeApps = _parser.ReadFile(Path.Combine(backupDir, $"mimeapps.list.{identifier}.bak"));
        WriteMimeApps(mimeApps);
        
        var pathBackup = Path.Combine(backupDir, $"environment.path.{identifier}.bak");
        var path = File.ReadAllText(pathBackup);
        PathEntry.Overwrite(path, out var oldPath, out var newPath);
        
        File.WriteAllText(Path.ChangeExtension(newBackupIdentifier, ".before.bak"), oldPath);
        File.WriteAllText(Path.ChangeExtension(newBackupIdentifier, ".after.bak"), newPath);
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
        WriteMimeApps(mimeApps, _mimeAppsPath);
    }
    
    private void WriteMimeApps(IniData mimeApps, string filePath)
    {
        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        
        _parser.WriteFile(filePath, mimeApps, Encoding.ASCII);
    }

    private static string GetMimeAppsPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mimeapps.list");
    }
}
