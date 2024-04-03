using Athena.Core.Runner;
using Athena.Resources;

namespace Athena.Core.Desktop.Linux;

internal static class DesktopEntry
{
    private static readonly string[] DesktopFiles = ["Desktop/athena.desktop", "Desktop/athena-gtk.desktop"];
    
    public static void Create(string desktopFileDir)
    {
        foreach (var desktopFile in DesktopFiles)
        {
            var resourceData = ResourceLoader.Load(desktopFile)
                .Replace("$PARAM_TYPE", "%u");

            var desktopFilePath = Path.Combine(desktopFileDir, desktopFile.Split('/').Last());
            File.WriteAllText(desktopFilePath, resourceData);
        }
    }
    
    public static void Delete(string desktopFileDir)
    {
        foreach (var desktopFile in DesktopFiles)
        {
            var desktopFilePath = Path.Combine(desktopFileDir, desktopFile.Split('/').Last());
            if (File.Exists(desktopFilePath)) File.Delete(desktopFilePath);
        }
    }
    
    public static void Source(string desktopFileDir, AppRunner appRunner)
    {
        appRunner.RunAsync("update-desktop-database", desktopFileDir).GetAwaiter().GetResult();
    }
}
