namespace Athena.Core.Desktop.Linux;

internal static class SymlinkEntry
{
    public static void Create(string symlinkPath, string appPath)
    {
        if (File.Exists(symlinkPath)) File.Delete(symlinkPath);
        
        File.CreateSymbolicLink(symlinkPath, appPath);
    }
    
    public static void Delete(string symlinkPath)
    {
        if (File.Exists(symlinkPath)) File.Delete(symlinkPath);
    }
}
