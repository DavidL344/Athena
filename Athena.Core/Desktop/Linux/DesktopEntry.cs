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
}
