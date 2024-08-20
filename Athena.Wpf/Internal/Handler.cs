using Athena.Core.Configuration;
using Athena.Core.Extensions;
using Athena.Core.Extensions.DependencyInjection;
using Athena.Core.Model;
using Athena.Core.Options;
using Athena.Core.Parser;
using Athena.Core.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Athena.Wpf.Internal;

public class Handler
{
    private readonly Config _config;
    private readonly PathParser _pathParser;
    private readonly OpenerParser _openerParser;
    private readonly AppEntryParser _appEntryParser;
    private readonly AppRunner _runner;
    
    public Handler()
    {
        var services = new ServiceCollection();
        services.AddAthenaCore();
        var serviceProvider = services.BuildServiceProvider();
        
        _config = serviceProvider.GetRequiredService<Config>();
        _pathParser = serviceProvider.GetRequiredService<PathParser>();
        _openerParser = serviceProvider.GetRequiredService<OpenerParser>();
        _appEntryParser = serviceProvider.GetRequiredService<AppEntryParser>();
        _runner = serviceProvider.GetRequiredService<AppRunner>();
    }

    public IOpener GetOpener(string filePath)
    {
        var parserOptions = new ParserOptions
        {
            AllowProtocols = _config.EnableProtocolHandler,
            StreamableProtocolPrefixes = _config.StreamableProtocolPrefixes,
            OpenLocally = false
        };
        
        var parsedPath = _pathParser.GetPath(filePath, parserOptions);
        return _openerParser.GetDefinition(parsedPath, parserOptions);
    }
    
    public int GetEntryId(IOpener opener)
    {
        var entryIndex = opener.GetAppEntryIndex(opener.DefaultApp);
        
        return entryIndex;
    }
    
    public int RunEntry(IOpener opener, int entryId, string filePath)
    {
        var entry = _appEntryParser.GetAppEntry(opener, entryId);
        entry = _appEntryParser.ExpandAppEntry(entry, filePath, _config.StreamableProtocolPrefixes);
        
        // In detached mode, the app picker will run the app and exit
        return _runner.Run(entry.Path, entry.Arguments, true);
    }
    
    public IEnumerable<string> GetFriendlyNames(IOpener opener)
    {
        return _appEntryParser.GetFriendlyNames(opener, multiline: true);
    }
}
