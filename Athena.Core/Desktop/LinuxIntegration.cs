using System.Reflection;
using System.Text;
using Athena.Core.Configuration;
using Athena.Core.Desktop.Linux;
using Athena.Core.Runner;
using IniParser;
using IniParser.Model;

namespace Athena.Core.Desktop;

public class LinuxIntegration : IDesktopIntegration
{
    private readonly AppRunner _appRunner;
    private readonly ConfigPaths _configPaths;
    
    // mimeapps.list
    private readonly string _mimeAppsPath;
    private readonly FileIniDataParser _parser;
    
    // athena.desktop, athena-gtk.desktop
    private readonly string[] _desktopFileNames;
    private readonly string _desktopFileDir;
    private readonly string[] _desktopFilePaths;
    
    // Athena.Cli <--> athena
    private readonly string[] _appPaths;
    private readonly string _symlinkDir;
    private readonly string[] _symlinkPaths;

    public LinuxIntegration(string mimeAppsPath, ConfigPaths configPaths,
        FileIniDataParser parser, AppRunner appRunner)
    {
        _appRunner = appRunner;
        _configPaths = configPaths;
        
        _desktopFileNames = ["athena.desktop", "athena-gtk.desktop"];
        _desktopFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local", "share", "applications");
        _desktopFilePaths = new string[_desktopFileNames.Length];
        
        for (var i = 0; i < _desktopFileNames.Length; i++)
        {
            _desktopFilePaths[i] = Path.Combine(_desktopFileDir, _desktopFileNames[i]);
        }
        
        // On Linux, the assembly's location points to its dll instead of the executable
        var assemblyLocation = Assembly.GetEntryAssembly()!.Location;
        _appPaths =
        [
            Path.ChangeExtension(assemblyLocation, null),
            Path.ChangeExtension(assemblyLocation
#if DEBUG
                    .Replace("Athena.Cli", "Athena.Gtk")
#endif
                , ".Gtk")
        ];

        _symlinkDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local", "bin");
        _symlinkPaths =
        [
            Path.Combine(_symlinkDir, "athena"),
            Path.Combine(_symlinkDir, "athena-gtk")
        ];
        
        _mimeAppsPath = mimeAppsPath;
        _parser = parser;
    }
    
    public LinuxIntegration(ConfigPaths configPaths, AppRunner appRunner) :
        this(GetMimeAppsPath(), configPaths, GetParserSettings(), appRunner) { }
    
    public void RegisterEntry()
    {
        // Create a directory for symlinks if it doesn't exist
        if (!Directory.Exists(_symlinkDir))
            Directory.CreateDirectory(_symlinkDir);
        
        // Add the symlink directory to $PATH
        BashRcEntry.Add(_symlinkDir);
        
        // Create a symlink to the executable in ~/.local/bin
        for (var i = 0; i < _symlinkPaths.Length; i++)
        {
            var symlinkPath = _symlinkPaths[i];
            SymlinkEntry.Create(symlinkPath, _appPaths[i]);
        }

        // Add a .desktop file to ~/.local/share/applications
        DesktopEntry.Create(_desktopFileDir);
        
        // Update the shell
        DesktopEntry.Source(_desktopFileDir, _appRunner);
    }
    
    public void DeregisterEntry()
    {
        BashRcEntry.Remove(_symlinkDir);
        foreach (var symlinkPath in _symlinkPaths)
            SymlinkEntry.Delete(symlinkPath);
        DesktopEntry.Delete(_desktopFileDir);
        
        // Update the shell
        DesktopEntry.Source(_desktopFileDir, _appRunner);
        
        // Only remove the symlink directory if it's empty
        if (!Directory.Exists(_symlinkDir)) return;
        if (Directory.GetFiles(_symlinkDir).Length != 0 || Directory.GetDirectories(_symlinkDir).Length != 0) return;
        Directory.Delete(_symlinkDir);
    }
    
    public void AssociateWithAllApps()
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.AssociateWithAllApps(
            mimeApps, _desktopFileNames[1]);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void AssociateWithSampleApps()
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.AssociateWithSampleApps(
            mimeApps, _desktopFileNames[1]);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void AssociateWithApp(string mimeType)
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.AssociateWithApp(
            mimeApps, mimeType, _desktopFileNames[1]);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void DissociateFromApps()
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.DissociateFromAllApps(
            mimeApps, _desktopFileNames[1]);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void DissociateFromApp(string fileExtensionOrMimeType)
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.DissociateFromApp(
            mimeApps, fileExtensionOrMimeType, _desktopFileNames[1]);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void SetAsDefault()
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.SetAsDefaultForAllMimeTypes(
            mimeApps, _desktopFileNames[1]);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void SetAsDefaultFor(string fileExtensionOrMimeType)
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.SetAsDefaultForMimeType(
            mimeApps, fileExtensionOrMimeType, _desktopFileNames[1]);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void UnsetDefault()
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.UnsetDefault(
            mimeApps, _desktopFileNames[1]);
        
        WriteMimeApps(modifiedMimeApps);
    }
    
    public void UnsetDefaultFrom(string fileExtensionOrMimeType)
    {
        var mimeApps = ReadMimeApps();
        
        var modifiedMimeApps = MimeAppsList.UnsetDefaultFrom(
            mimeApps, fileExtensionOrMimeType, _desktopFileNames[1]);
        
        WriteMimeApps(modifiedMimeApps);
    }

    public void BackupAllEntries(string backupDir, string identifier)
    {
        var mimeApps = ReadMimeApps();
        WriteMimeApps(mimeApps, Path.Combine(backupDir, $"mimeapps.list.{identifier}.bak"));

        var pathBackup = Path.Combine(backupDir, $"path.{identifier}.bak");
        File.WriteAllText(pathBackup, BashRcEntry.Get());
    }
    
    public void RestoreAllEntries(string backupDir, string identifier)
    {
        var mimeApps = _parser.ReadFile(Path.Combine(backupDir, $"mimeapps.list.{identifier}.bak"));
        WriteMimeApps(mimeApps);
    }

    public string ConsoleStatus()
    {
        var status = new LinuxStatus
        {
            AppPath = _appPaths[0],
            SymlinkPath = _symlinkPaths[0],
            DesktopFilePath = _desktopFilePaths[0],
            ConfigDir = _configPaths.Root
        };
        
        return status.ToSpectreConsole();
    }
    
    private IniData ReadMimeApps()
    {
        if (File.Exists(_mimeAppsPath)) return _parser.ReadFile(_mimeAppsPath);
            
        WriteMimeApps(MimeAppsList.Create());

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
    
    private static FileIniDataParser GetParserSettings()
    {
        return new FileIniDataParser { Parser = { Configuration = { AssigmentSpacer = "" } } };
    }
}
