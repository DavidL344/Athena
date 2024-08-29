namespace Athena.Desktop.System.Windows;

public static class PathEntry
{
    public static void Add(string dirToAdd)
    {
        var pathVariable = GetPath();
        if (pathVariable.Contains(dirToAdd))
            return;

        var newPath = pathVariable.EndsWith(';')
            ? $"{pathVariable}{dirToAdd};"
            : $"{pathVariable};{dirToAdd}";
        
        SetPath(newPath);
    }
    
    public static void Remove(string dirToRemove)
    {
        var pathVariable = GetPath();
        if (!pathVariable.Contains(dirToRemove))
            return;

        var newPath = pathVariable.EndsWith(';')
            ? pathVariable.Replace($"{dirToRemove};", "")
            : pathVariable.Replace($";{dirToRemove}", "");
        
        SetPath(newPath);
    }
    
    private static string GetPath()
    {
        var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        
        if (path is null)
            throw new ArgumentNullException(nameof(path), "The PATH environment variable is not set!");
        
        return path;
    }
    
    private static void SetPath(string newPath)
    {
        Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
    }
}
