using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Athena.Core.Desktop.Windows;

[SupportedOSPlatform("windows")]
public class RegistryEntry
{
    private const string ClassesPath = @"Software\Classes";
    
    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    
    public static void AddToContextMenu(Type type, string value, string appPath, string args)
    {
        if (value is "http" or "https")
            throw new NotSupportedException($"Modifying the {value.ToUpper()} protocol is not supported!");
        
        switch (type)
        {
            case Type.FileExtension:
                AddFileExtensionToContextMenu(value, appPath, args);
                break;
            case Type.Protocol:
                AddProtocolToContextMenu(value, appPath, args);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
    
    public static void RemoveFromContextMenu(Type type, string value)
    {
        if (value is "http" or "https")
            throw new NotSupportedException($"Modifying the {value.ToUpper()} protocol is not supported!");
        
        switch (type)
        {
            case Type.FileExtension:
                RemoveFileExtensionFromContextMenu(value);
                break;
            case Type.Protocol:
                RemoveProtocolFromContextMenu(value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private static void AddFileExtensionToContextMenu(string value, string appPath, string args)
    {
        var appPathWithArgs = $"\"{appPath}\" {args.Replace("%1", "\"%1\"")}";
        var keyPath = $@"{ClassesPath}\{value}";
        
        // HKCU\Software\Classes\*
        if (!KeyExists(keyPath))
        {
            using var key = Registry.CurrentUser.CreateSubKey(keyPath);
            key.SetValue("Added by", "Athena");
        }
        
        // HKCU\Software\Classes\*\shell\OpenWithAthena
        using var allOpenKey = Registry.CurrentUser.CreateSubKey($@"{ClassesPath}\{value}\shell\OpenWithAthena");
        allOpenKey.SetValue(null, "Open with Athena");
        allOpenKey.SetValue("Icon", $"\"{appPath}\",0");
        allOpenKey.SetValue("Extended", "");
        
        // HKCU\Software\Classes\*\shell\OpenWithAthena\command
        using var allOpenCommandKey = Registry.CurrentUser.CreateSubKey($@"{ClassesPath}\{value}\shell\OpenWithAthena\command");
        allOpenCommandKey.SetValue(null, appPathWithArgs);
    }
    
    private static void AddProtocolToContextMenu(string value, string appPath, string args)
    {
        var appPathWithArgs = $"\"{appPath}\" {args.Replace("%1", "\"%1\"")}";
        var keyPath = $@"{ClassesPath}\{value}";
        
        // HKCU\Software\Classes\*
        if (!KeyExists(keyPath))
        {
            using var key = Registry.CurrentUser.CreateSubKey(keyPath);
            key.SetValue(null, $"URL:{value}");
            key.SetValue("URL Protocol", "");
            key.SetValue("Added by", "Athena");
        }
        
        BackupAndReplaceOpenCommand(keyPath, appPathWithArgs);
    }
    
    private static void RemoveFileExtensionFromContextMenu(string fileExtension)
    {
        var keyPath = $@"{ClassesPath}\{fileExtension}";

        using var key = Registry.CurrentUser.OpenSubKey(keyPath, false);
        if (key is null) return;
        
        var fullDeletion = WasAddedByAthena(key);
        key.Close();
        
        // HKCU\Software\Classes\*
        if (fullDeletion)
        {
            Registry.CurrentUser.DeleteSubKeyTree(keyPath);
            return;
        }
        
        // HKCU\Software\Classes\*\shell\OpenWithAthena
        Registry.CurrentUser.DeleteSubKeyTree($@"{keyPath}\shell\OpenWithAthena");
    }
    
    private static void RemoveProtocolFromContextMenu(string protocol)
    {
        var keyPath = $@"{ClassesPath}\{protocol}";
        
        using var protocolKey = Registry.CurrentUser.OpenSubKey(keyPath, false);
        if (protocolKey is null) return;
        
        var fullDeletion = WasAddedByAthena(protocolKey);
        protocolKey.Close();
        
        // HKCU\Software\Classes\*
        if (fullDeletion)
        {
            Registry.CurrentUser.DeleteSubKeyTree(keyPath);
            return;
        }
        
        // HKCU\Software\Classes\*\shell\OpenWithAthena
        RestoreOpenCommand(keyPath);
    }
    
    private static void BackupAndReplaceOpenCommand(string keyPath, string openCommand)
    {
        var commandKeyPath = $@"{keyPath}\shell\open\command";
        
        using var regKey = Registry.CurrentUser.CreateSubKey(commandKeyPath, true);
        
        if (regKey.GetValue(null) is string value)
        {
            regKey.SetValue("Athena.Backup", value);
        }
        regKey.SetValue(null, openCommand);
    }
    
    private static void RestoreOpenCommand(string keyPath)
    {
        var commandKeyPath = $@"{keyPath}\shell\open\command";
        
        using var regKey = Registry.CurrentUser.OpenSubKey(commandKeyPath, true);
        if (regKey?.GetValue("Athena.Backup") is not string backupValue) return;
        
        regKey.SetValue(null, backupValue);
        regKey.DeleteValue("Athena.Backup");
    }
    
    public static void Source()
    {
        const int SHCNE_ASSOCCHANGED = 0x08000000;
        const uint SHCNF_IDLIST = 0x0000;
        
        SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
    }
    
    private static bool KeyExists(string key)
    {
        using var regKey = Registry.CurrentUser.OpenSubKey(key, false);
        return regKey is not null;
    }
    
    private static bool WasAddedByAthena(RegistryKey key)
    {
        return key.GetValue("Added by") as string == "Athena";
    }
    
    public enum Type
    {
        FileExtension,
        Protocol
    }
}
