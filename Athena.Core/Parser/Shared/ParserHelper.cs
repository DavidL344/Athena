using System.Text.RegularExpressions;

namespace Athena.Core.Parser.Shared;

internal class ParserHelper
{
    public static bool IsLocalOrRequested(string filePath, bool openLocally,
        IEnumerable<string> streamableProtocolPrefixes)
        => new Uri(filePath).IsFile || openLocally || IsStreamable(filePath, streamableProtocolPrefixes);
    
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
    
    private static string RemoveProtocolFromUrl(string url)
    {
        var uri = new Uri(url);
        var protocol = uri.Scheme;
        
        var pattern = $@"(^{protocol}:(?:\/{{0,2}}))?";
        var result = Regex.Replace(url, pattern, "");
        
        return result;
    }
}