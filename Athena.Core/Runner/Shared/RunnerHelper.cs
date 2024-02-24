namespace Athena.Core.Runner.Shared;

internal class RunnerHelper
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
}
