using System.Reflection;
using Athena.Core.Model.Opener;
using Athena.Core.Parser;
using Athena.Core.Parser.Options;
using Athena.Core.Parser.Shared;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Tests.Parser;

public class OpenerParserTests : IDisposable
{
    private readonly string _testsConfigDir;
    private readonly Dictionary<ConfigType, string> _configPaths;
    private readonly ILogger<OpenerParser> _logger;

    public OpenerParserTests()
    {
        var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        _testsConfigDir = Path.Combine(workingDir, "user-opener-parser-tests");
        _configPaths = new Dictionary<ConfigType, string>
        {
            { ConfigType.Entries, Path.Combine(_testsConfigDir, "entries") },
            { ConfigType.Files, Path.Combine(_testsConfigDir, "files") },
            { ConfigType.Protocols, Path.Combine(_testsConfigDir, "protocols") }
        };
        _logger = new Logger<OpenerParser>(new LoggerFactory());
        
        Internal.Samples.Generate(_testsConfigDir).GetAwaiter().GetResult();
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Internal.Samples.Remove(_testsConfigDir);
    }
    
    [Theory]
    [InlineData("/file.mp4")]
    [InlineData("/home/file.mp4")]
    [InlineData("C:/file.mp4")]
    public async Task GetOpenerDefinition__ReturnsFileExtensionDefinition__WhenFilePathIsSpecified(string filePath)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new OpenerParser(_configPaths, options, _logger);
        var expected = new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play", "vlc.play" ]
        };
        
        // Act
        var result = await parser.GetOpenerDefinition(filePath);
        
        // Assert
        Assert.Equivalent(expected, result);
    }
    
    [Theory]
    [InlineData("file:///file.mp4")]
    [InlineData("file://file.mp4")]
    [InlineData("file:///C:/file.mp4")]
    [InlineData("file://C:/file.mp4")]
    [InlineData("file:///home/file.mp4")]
    [InlineData("file://home/file.mp4")]
    public async Task GetOpenerDefinition__ReturnsFileExtensionDefinition__WhenUriIsLocal(string filePath)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new OpenerParser(_configPaths, options, _logger);
        var expected = new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play", "vlc.play" ]
        };
        
        // Act
        var result = await parser.GetOpenerDefinition(filePath);
        
        // Assert
        Assert.Equivalent(expected, result);
    }

    [Theory]
    [InlineData("https://example.com/file.mp4")]
    public async Task GetOpenerDefinition__ReturnsFileExtensionDefinition__WhenLocalOverrideRequested(string url)
    {
        // Arrange
        var options = new ParserOptions { OpenLocally = true };
        var parser = new OpenerParser(_configPaths, options, _logger);
        var expected = new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play", "vlc.play" ]
        };
        
        // Act
        var result = await parser.GetOpenerDefinition(url);
        
        // Assert
        Assert.Equivalent(expected, result);
    }

    [Theory]
    [InlineData("athena:https://example.com/file.mp4")]
    [InlineData("athena:/https://example.com/file.mp4")]
    [InlineData("athena://https://example.com/file.mp4")]
    [InlineData("stream:https://example.com/file.mp4")]
    [InlineData("stream:/https://example.com/file.mp4")]
    [InlineData("stream://https://example.com/file.mp4")]
    public async Task GetOpenerDefinition__ReturnsFileExtensionDefinition__WhenUrlIsStreamable(string url)
    {
        // Arrange
        var options = new ParserOptions { StreamableProtocolPrefixes = [ "athena", "stream" ] };
        var parser = new OpenerParser(_configPaths, options, _logger);
        var expected = new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play", "vlc.play" ]
        };
        
        // Act
        var result = await parser.GetOpenerDefinition(url);
        
        // Assert
        Assert.Equivalent(expected, result);
    }
    
    [Theory]
    [InlineData("http://example.com/file.mp4")]
    [InlineData("https://example.com/file.mp4")]
    public async Task GetOpenerDefinition__ReturnsProtocolDefinition__WhenUriIsRemote(string url)
    {
        // Arrange
        var uri = new Uri(url);
        var options = new ParserOptions { AllowProtocols = true };
        var parser = new OpenerParser(_configPaths, options, _logger);
        var expected = new Protocol
        {
            Name = uri.Scheme,
            AppList =
            [
                "firefox.open",
                "firefox.open-private",
                "chromium.open",
                "chromium.open-private"
            ]
        };
        
        // Act
        var result = await parser.GetOpenerDefinition(url);
        
        // Assert
        Assert.Equivalent(expected, result);
    }
    
    [Theory]
    [InlineData("https://example.com/file.mp4")]
    public async Task GetOpenerDefinition__ThrowsException__WhenProtocolHandlerIsDisabled(string url)
    {
        // Arrange
        var options = new ParserOptions { AllowProtocols = false };
        var parser = new OpenerParser(_configPaths, options, _logger);
        
        // Act
        var exception = await Record.ExceptionAsync(() => parser.GetOpenerDefinition(url));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal("The protocol handler is disabled!", exception.Message);
    }
    
    [Theory]
    [InlineData("/file.random")]
    [InlineData("https://example.com/file.random")]
    public async Task GetOpenerDefinition__ThrowsException__WhenFileExtensionIsNotRegistered(string filePath)
    {
        // Arrange
        var options = new ParserOptions { OpenLocally = true, AllowProtocols = true};
        var parser = new OpenerParser(_configPaths, options, _logger);
        
        // Act
        var exception = await Record.ExceptionAsync(() => parser.GetOpenerDefinition(filePath));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal("The file extension (random) isn't registered with Athena!", exception.Message);
    }
}
