using System.Runtime.Versioning;

namespace Athena.Desktop.System.Linux;

[SupportedOSPlatform("linux")]
internal static class SymlinkEntry
{
    public static void Create(string symlinkPath, string appPath)
    {
        if (File.Exists(symlinkPath)) File.Delete(symlinkPath);

        var symlinkDir = Path.GetDirectoryName(symlinkPath);
        
        if (!Directory.Exists(Path.GetDirectoryName(symlinkPath))
            && !string.IsNullOrEmpty(symlinkDir))
            Directory.CreateDirectory(symlinkDir);
        
        File.CreateSymbolicLink(symlinkPath, appPath);
    }
    
    public static void Delete(string symlinkPath)
    {
        if (!Directory.Exists(Path.GetDirectoryName(symlinkPath)))
            return;
        
        if (File.Exists(symlinkPath)) File.Delete(symlinkPath);
    }
}
