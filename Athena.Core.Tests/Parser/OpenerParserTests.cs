using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athena.Core.Internal;
using Athena.Core.Model;
using Athena.Core.Options;
using Athena.Core.Parser;
using Athena.Desktop.Configuration;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Tests.Parser;

public class OpenerParserTests : IDisposable
{
    private readonly string _testsConfigDir;
    private readonly ConfigPaths _configPaths;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<OpenerParser> _logger;

    public OpenerParserTests()
    {
        var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        _testsConfigDir = Path.Combine(workingDir, "user-opener-parser-tests");
        _configPaths = new ConfigPaths(_testsConfigDir);
        
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = false,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        _logger = new Logger<OpenerParser>(new LoggerFactory());
        
        Startup.CheckEntries(new ConfigPaths(_testsConfigDir), _jsonSerializerOptions)
            .GetAwaiter().GetResult();
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        if (Directory.Exists(_testsConfigDir))
            Directory.Delete(_testsConfigDir, true);
    }
    
    [Theory]
    [InlineData("/file.mp4")]
    [InlineData("/home/file.mp4")]
    [InlineData("C:/file.mp4")]
    public void GetOpenerDefinition__ReturnsFileExtensionDefinition__WhenFilePathIsSpecified(string filePath)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new OpenerParser(_configPaths, _jsonSerializerOptions, _logger);
        var expected = new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play", "vlc.play" ]
        };
        
        // Act
        var result = parser.GetDefinition(filePath, options);
        
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
    public void GetOpenerDefinition__ReturnsFileExtensionDefinition__WhenUriIsLocal(string filePath)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new OpenerParser(_configPaths, _jsonSerializerOptions, _logger);
        var expected = new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play", "vlc.play" ]
        };
        
        // Act
        var result = parser.GetDefinition(filePath, options);
        
        // Assert
        Assert.Equivalent(expected, result);
    }

    [Theory]
    [InlineData("https://example.com/file.mp4")]
    public void GetOpenerDefinition__ReturnsFileExtensionDefinition__WhenLocalOverrideRequested(string url)
    {
        // Arrange
        var options = new ParserOptions { OpenLocally = true };
        var parser = new OpenerParser(_configPaths, _jsonSerializerOptions, _logger);
        var expected = new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play", "vlc.play" ]
        };
        
        // Act
        var result = parser.GetDefinition(url, options);
        
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
    public void GetOpenerDefinition__ReturnsFileExtensionDefinition__WhenUrlIsStreamable(string url)
    {
        // Arrange
        var options = new ParserOptions { StreamableProtocolPrefixes = [ "athena", "stream" ] };
        var parser = new OpenerParser(_configPaths, _jsonSerializerOptions, _logger);
        var expected = new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play", "vlc.play" ]
        };
        
        // Act
        var result = parser.GetDefinition(url, options);
        
        // Assert
        Assert.Equivalent(expected, result);
    }
    
    [Theory]
    [InlineData("http://example.com/file.mp4")]
    [InlineData("https://example.com/file.mp4")]
    public void GetOpenerDefinition__ReturnsProtocolDefinition__WhenUriIsRemote(string url)
    {
        // Arrange
        var uri = new Uri(url);
        var options = new ParserOptions { AllowProtocols = true };
        var parser = new OpenerParser(_configPaths, _jsonSerializerOptions, _logger);
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
        var result = parser.GetDefinition(url, options);
        
        // Assert
        Assert.Equivalent(expected, result);
    }
    
    [Theory]
    [InlineData("https://example.com/file.mp4")]
    public void GetOpenerDefinition__ThrowsException__WhenProtocolHandlerIsDisabled(string url)
    {
        // Arrange
        var options = new ParserOptions { AllowProtocols = false };
        var parser = new OpenerParser(_configPaths, _jsonSerializerOptions, _logger);
        
        // Act
        var exception = Record.Exception(() => parser.GetDefinition(url, options));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal("The protocol handler is disabled!", exception.Message);
    }
    
    [Theory]
    [InlineData("/file.random")]
    [InlineData("https://example.com/file.random")]
    public void GetOpenerDefinition__ThrowsException__WhenFileExtensionIsNotRegistered(string filePath)
    {
        // Arrange
        var options = new ParserOptions { OpenLocally = true, AllowProtocols = true};
        var parser = new OpenerParser(_configPaths, _jsonSerializerOptions, _logger);
        
        // Act
        var exception = Record.Exception(() => parser.GetDefinition(filePath, options));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal("The file extension (random) isn't registered with Athena!", exception.Message);
    }
}
