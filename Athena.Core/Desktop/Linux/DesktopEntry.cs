using Athena.Core.Runner;
using Athena.Resources;

namespace Athena.Core.Desktop.Linux;

internal static class DesktopEntry
{
    public static void Create(string desktopFilePath)
    {
        var desktopFile = ResourceLoader.Load("Desktop/athena.desktop")
            .Replace("$PARAM_TYPE", "%u");
        
        File.WriteAllText(desktopFilePath, desktopFile);
    }
    
    public static void Delete(string desktopFilePath)
    {
        if (File.Exists(desktopFilePath)) File.Delete(desktopFilePath);
    }
    
    public static void Source(string directory, AppRunner appRunner)
    {
        appRunner.RunAsync("update-desktop-database", directory).GetAwaiter().GetResult();
    }
}
