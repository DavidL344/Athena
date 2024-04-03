using Athena.Core.Runner;
using Athena.Resources;

namespace Athena.Core.Desktop.Linux;

internal static class BashRcEntry
{
    private static readonly string BashRcPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".bashrc");
    
    public static void Add(string dirToAdd)
    {
        dirToAdd = dirToAdd.Replace(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "$HOME");
        
        var bashrcPath = BashRcPath;
        var bashrcData = File.ReadAllText(bashrcPath);
        
        var appendedLines = ResourceLoader.Load("Desktop/bashrc.sh")
            .Replace("$SYMLINK_DIR", $"\"{dirToAdd}\"")
            .Replace("$ATHENA_PATH", "athena");
        
        appendedLines = $"{Environment.NewLine}{appendedLines}";
        
        if (bashrcData.Contains(appendedLines))
            return;
        
        File.AppendAllText(bashrcPath, appendedLines);
    }

    public static void Remove(string dirToRemove)
    {
        dirToRemove = dirToRemove.Replace(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "$HOME");
        
        var bashrcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".bashrc");
        var bashrcData = File.ReadAllText(bashrcPath);
        
        var appendedLines = ResourceLoader.Load("Desktop/bashrc.sh")
            .Replace("$SYMLINK_DIR", $"\"{dirToRemove}\"")
            .Replace("$ATHENA_PATH", "athena");
        
        if (!bashrcData.Contains(appendedLines))
            return;
        
        bashrcData = bashrcData
            .Replace($"{Environment.NewLine}{appendedLines}", "")
            .Replace(appendedLines, "");
        
        File.WriteAllText(bashrcPath, bashrcData);
    }

    public static void Source(AppRunner appRunner)
    {
        appRunner.RunAsync("bash", $"-c \"source {BashRcPath}\"").GetAwaiter().GetResult();
    }
    
    public static string Get()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        
        if (path is null)
            throw new ArgumentNullException(nameof(path), "The PATH environment variable is not set!");
        
        return path;
    }
}
