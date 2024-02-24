using Athena.Core.Parser.Options;
using Athena.Core.Parser.Shared;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Parser;

public class PathParser
{
    private readonly ParserOptions _options;
    private readonly ILogger _logger;

    public PathParser(ParserOptions options, ILogger<PathParser> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Uri GetUri(string filePath)
    {
        filePath = filePath
            .Replace("/", Path.DirectorySeparatorChar.ToString())
            .Replace(@"\", Path.DirectorySeparatorChar.ToString());

        var expandedPath = ParserHelper.ExpandEnvironmentVariables(filePath);
        Uri uri;
        
        try
        {
            uri = new Uri(expandedPath);
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
            return new Uri(expandedPath);
        
        // The URI is a local file or the user wants to open a URL locally,
        // based on its file extension instead of the protocol
        if (ParserHelper.IsLocalOrRequested(expandedPath, _options.OpenLocally, _options.StreamableProtocolPrefixes))
            return new Uri(Path.GetFullPath(expandedPath));
        
        return new Uri(expandedPath);
    }
}
