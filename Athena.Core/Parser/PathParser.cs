using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Athena.Core.Internal.Helpers;
using Athena.Core.Options;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Parser;

public partial class PathParser
{
    private readonly ILogger _logger;

    public PathParser(ILogger<PathParser> logger)
    {
        _logger = logger;
    }

    public string GetPath(string filePath, ParserOptions options)
    {
        var expandedPath = ParserHelper.ExpandEnvironmentVariables(filePath);
        Uri uri;
        
        // Prevent the backslashes from expanding the file path as a relative path
        if (expandedPath.StartsWith('\\'))
            expandedPath = expandedPath.Replace('\\', '/');
        
        try
        {
            uri = new Uri(expandedPath);
            if (!expandedPath.Contains("://") && !expandedPath.Contains(@":\\"))
                expandedPath = expandedPath.Replace('/', Path.DirectorySeparatorChar);
            
            _logger.LogDebug("Parsed {FilePath} as an absolute path: {FileUri}",
                filePath, uri);
        }
        catch (UriFormatException)
        {
            // The path starts with a dot or contains just the file name
            expandedPath = Path.GetFullPath(expandedPath);
            uri = new Uri(expandedPath);
            
            _logger.LogDebug("Parsed {FilePath} as either a relative path or a URL: {FileUri}",
                filePath, uri);
        }
        
        // The URI is a local file or a Windows-style absolute path
        // (either relative or absolute, and either with or without a drive letter)
        if (!expandedPath.StartsWith(Path.DirectorySeparatorChar)
            && !expandedPath.StartsWith('.'))
        {
            // On Linux, convert the drive letter to the root directory
            if (OperatingSystem.IsLinux())
                expandedPath = ConvertDriveLetter(expandedPath, filePath);
            
            return expandedPath;
        }
        
        // The URI is a local file or the user wants to open a URL locally,
        // based on its file extension instead of the protocol
        if (ParserHelper.IsLocalOrRequested(expandedPath, options.OpenLocally, options.StreamableProtocolPrefixes))
            return Path.GetFullPath(expandedPath);
        
        return expandedPath;
    }

#if !LINUX
    [ExcludeFromCodeCoverage]
#endif
    private static string ConvertDriveLetter(string expandedPath, string filePath)
    {
        var windowsDriveRegex = WindowsDriveRegex();
        var windowsDriveMatch = windowsDriveRegex.Match(filePath);
            
        if (windowsDriveMatch.Success)
            expandedPath = $"/{expandedPath.Remove(0, windowsDriveMatch.Length)}";
                
        if (!expandedPath.StartsWith('\\'))
            expandedPath = expandedPath.Replace('\\', Path.DirectorySeparatorChar);
        
        return expandedPath;
    }
    
    [GeneratedRegex(@"^[a-zA-Z]:[\\|\/]")]
    private static partial Regex WindowsDriveRegex();
}
