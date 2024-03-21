namespace Athena.Core.Desktop.Shared;

internal static class PathEntry
{
    public static void Add(string dirToAdd)
    {
        var pathVariable = GetPath();
        if (pathVariable.Contains(dirToAdd))
            return;
        
        var newPath = OperatingSystem.IsWindows()
            ? pathVariable.EndsWith(';')
                ? $"{pathVariable}{dirToAdd};"
                : $"{pathVariable};{dirToAdd}"
            : $"{pathVariable}:{dirToAdd}";
        
        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
    }

    public static void Remove(string dirToRemove)
    {
        var pathVariable = GetPath();
        if (!pathVariable.Contains(dirToRemove))
            return;
        
        var newPath = OperatingSystem.IsWindows()
            ? pathVariable.EndsWith(';')
                ? pathVariable.Replace($";{dirToRemove};", ";")
                : pathVariable.Replace($"{dirToRemove};", "")
            : pathVariable.Replace($"{dirToRemove}:", "")
                .Replace($":{dirToRemove}", "");

        _ = SetPath(newPath);
    }

    public static string Get() => GetPath();
    
    public static void Overwrite(string path, out string oldPath, out string newPath)
    {
        oldPath = GetPath();
        newPath = SetPath(path);
    }

    private static string GetPath()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        
        if (path is null)
            throw new ArgumentNullException(nameof(path), "The PATH environment variable is not set!");
        
        return path;
    }
    
    private static string SetPath(string newPath)
    {
        Environment.SetEnvironmentVariable("PATH", newPath);
        return GetPath();
    }
}
