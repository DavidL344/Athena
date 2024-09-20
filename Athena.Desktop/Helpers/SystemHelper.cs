namespace Athena.Desktop.Helpers;

public class SystemHelper
{
    public static string WhereIs(string command)
    {
        var paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process)?.Split(Path.PathSeparator);
        if (paths is null)
            throw new ApplicationException("Environment variable 'PATH' not found!");
        
        string[] executableFileExtensions = [".exe", ".bat", ".cmd", ".ps1"];
        if (OperatingSystem.IsWindows() && !EndsWithAtLeastOne(command, executableFileExtensions))
        {
            foreach (var fileExtension in executableFileExtensions)
            {
                foreach (var path in paths)
                {
                    var fullPath = Path.Combine(path, $"{command}{fileExtension}");
                    if (File.Exists(fullPath))
                        return fullPath;
                }
            }
            
            return string.Empty;
        }
        
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

    private static bool EndsWithAtLeastOne(string @string, IEnumerable<string> anyOccurence)
        => anyOccurence.Any(@string.EndsWith);
}
