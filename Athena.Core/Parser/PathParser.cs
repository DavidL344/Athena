using Athena.Core.Parser.Options;
using Athena.Core.Parser.Shared;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Parser;

public class PathParser
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
            expandedPath = expandedPath.Replace('/', Path.DirectorySeparatorChar);
            
            _logger.LogInformation("Parsed {FilePath} as an absolute path: {FileUri}",
                filePath, uri);
        }
        catch (UriFormatException)
        {
            // The path starts with a dot or contains just the file name
            expandedPath = Path.GetFullPath(expandedPath);
            uri = new Uri(expandedPath);
            
            _logger.LogInformation("Parsed {FilePath} as either a relative path or a URL: {FileUri}",
                filePath, uri);
        }
        
        // The URI is a local file or a Windows-style absolute path
        // (either relative or absolute, and either with or without a drive letter)
        if (!expandedPath.StartsWith(Path.DirectorySeparatorChar)
            && !expandedPath.StartsWith('.'))
            return expandedPath;
        
        // The URI is a local file or the user wants to open a URL locally,
        // based on its file extension instead of the protocol
        if (ParserHelper.IsLocalOrRequested(expandedPath, options.OpenLocally, options.StreamableProtocolPrefixes))
            return Path.GetFullPath(expandedPath);
        
        return expandedPath;
    }
}
