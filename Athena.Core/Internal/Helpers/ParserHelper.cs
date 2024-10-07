using System.Text.RegularExpressions;

namespace Athena.Core.Internal.Helpers;

internal class ParserHelper
{
    public static bool IsLocalOrRequested(string filePath, bool openLocally,
        IEnumerable<string> streamableProtocolPrefixes)
    {
        if (filePath.StartsWith('/') && OperatingSystem.IsWindows())
            filePath = Path.GetFullPath(filePath);
        
        return new Uri(filePath).IsFile || openLocally || IsStreamable(filePath, streamableProtocolPrefixes);
    }

    public static bool IsStreamable(string url, IEnumerable<string> streamableProtocolPrefixes)
        => streamableProtocolPrefixes.Any(protocol => url.StartsWith($"{protocol}:"));
    
    public static string ParseStreamPath(string filePath, IEnumerable<string> streamableProtocolPrefixes)
    {
        if (filePath.StartsWith("file:"))
            return Path.GetFullPath(RemoveProtocolFromUrl(filePath));
        
        if (IsStreamable(filePath, streamableProtocolPrefixes))
            return RemoveProtocolFromUrl(filePath);
        
        return filePath;
    }
    
    public static string RemoveProtocolFromUrl(string url)
    {
        var uri = new Uri(url);
        var protocol = uri.Scheme;
        
        var pattern = $@"(^{protocol}:(?:\/{{0,2}}))?";
        var result = Regex.Replace(url, pattern, "");
        
        return result;
    }
    
    public static string ExpandEnvironmentVariables(string filePath)
    {
        var expandedPath = filePath
            .Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        
        // Replace $VAR with %VAR% to support environment variable expansion
        var regex = new Regex(@"\$(\w+)");
        var matches = regex.Matches(expandedPath).ToArray();
        foreach (var match in matches)
        {
            var variable = match.Groups[1].Value;
            var isNumber = int.TryParse(variable, out _);
            
            expandedPath = expandedPath
                .Replace($"${variable}",isNumber
                    ? $"%{variable}"
                    : $"%{variable}%");
        }
        
        return Environment.ExpandEnvironmentVariables(expandedPath);
    }

    public static string ExpandEnvironmentVariables(string args, string filePath)
    {
        return ExpandEnvironmentVariables(args
            .Replace("$FILE", $"\"{filePath}\"")
            .Replace("$URL", $"\"{filePath}\"")
            .Replace("%1", $"\"{filePath}\"")
            .Replace("$1", $"\"{filePath}\""));
    }
}
