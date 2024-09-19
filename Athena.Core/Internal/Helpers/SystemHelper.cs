namespace Athena.Core.Internal.Helpers;

internal class SystemHelper
{
    public static string WhereIs(string command)
    {
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator);
        if (paths is null)
            throw new ApplicationException("Environment variable 'PATH' not found!");
        
        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, command);
            if (File.Exists(fullPath))
                return fullPath;
        }
        
        return string.Empty;
    }

    public static bool IsInPath(string command)
        => !string.IsNullOrEmpty(WhereIs(command));

    public static string GetDefaultEditor()
    {
        string[] editors = OperatingSystem.IsWindows()
            ? ["code.open", "notepad.open"]
            : ["codium.open", "gedit.open", "nano.open", "vi.open"];
        
        foreach (var editor in editors)
        {
            if (IsInPath(editor)) return editor;
        }
        
        return editors.Last();
    }
}
