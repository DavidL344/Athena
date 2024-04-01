using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athena.Core.Configuration;
using Athena.Core.Internal;
using Athena.Core.Model;
using Athena.Core.Options;
using Athena.Core.Parser;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Tests.Parser;

public class AppEntryParserTests : IDisposable
{
    private readonly string _testsConfigDir;
    private readonly ConfigPaths _configPaths;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Logger<AppEntryParser> _logger;

    public AppEntryParserTests()
    {
        var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        _testsConfigDir = Path.Combine(workingDir, "user-app-entry-parser-tests");
        _configPaths = new ConfigPaths(_testsConfigDir);
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = false,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        _logger = new Logger<AppEntryParser>(new LoggerFactory());
        
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
    [InlineData("/file.mp4", true)]
    [InlineData("/file.mp4", false)]
    public void GetAppDefinition__ReturnsAppDefinition__WhenItExists(string filePath, bool expandVars)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new AppEntryParser(_configPaths, _jsonSerializerOptions, _logger);
        var expected = new AppEntry
        {
            Name = "mpv (Play)",
            Path = "mpv",
            Arguments = expandVars ? $"\"{filePath}\"" : "$FILE"
        };
        
        // Act
        var appEntry = parser.GetAppEntry(new FileExtension
        {
            Name = "MP4 Video",
            AppList = ["mpv.play"]
        }, 0);
        var result = expandVars
            ? parser.ExpandAppEntry(appEntry, filePath, options.StreamableProtocolPrefixes)
            : appEntry;
        
        // Assert
        Assert.Equivalent(expected, result);
    }
    
    [Theory]
    [InlineData("mpv.pause")]
    public void GetAppDefinition__ThrowsException__WhenItDoesNotExist(string entryName)
    {
        // Arrange
        var parser = new AppEntryParser(_configPaths, _jsonSerializerOptions, _logger);
        
        // Act
        var exception = Record.Exception(() => parser.GetAppEntry(entryName));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal($"The app entry ({entryName}) isn't registered with Athena!",
            exception.Message);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void GetAppDefinition__ThrowsException__WhenIndexIsOutOfRange(int entryIndex)
    {
        // Arrange
        var parser = new AppEntryParser(_configPaths, _jsonSerializerOptions, _logger);
        
        // Act
        var exception = Record.Exception(() => parser.GetAppEntry(new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play" ]
        }, entryIndex));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal("The entry ID is out of range!", exception.Message);
    }
    
    [Theory]
    [InlineData("random.play")]
    [InlineData("random.open")]
    public void GetAppDefinition__ThrowsException__WhenAppDefinitionDoesNotExist(string definitionName)
    {
        // Arrange
        var parser = new AppEntryParser(_configPaths, _jsonSerializerOptions, _logger);
        
        // Act
        var exception = Record.Exception(() => parser.GetAppEntry(definitionName));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal($"The app entry ({definitionName}) isn't registered with Athena!", exception.Message);
    }

    [Theory]
    [InlineData("https://example.com/file.temp", true)]
    [InlineData("https://example.com/file.temp", false)]
    public void GetAppDefinition__RemovesProtocol__WhenRequested(string url, bool removeProtocol)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new AppEntryParser(_configPaths, _jsonSerializerOptions, _logger);
        var expected = new AppEntry
        {
            Name = "mpv (Play)",
            Path = "mpv",
            Arguments = removeProtocol ? url : "$FILE",
            RemoveProtocol = removeProtocol
        };
        
        // Act
        var filePath = Path.Combine(_configPaths.Subdirectories[ConfigType.AppEntries], ".temp.open.json");
        var fileContents = JsonSerializer.Serialize(expected, _jsonSerializerOptions);
        File.WriteAllText(filePath, fileContents);
        
        var appEntry = parser.GetAppEntry(new FileExtension
        {
            Name = "A temporary file",
            AppList = [ ".temp.open" ]
        }, 0);
        var result = removeProtocol
            ? parser.ExpandAppEntry(appEntry, url, options.StreamableProtocolPrefixes)
            : appEntry;
        File.Delete(filePath);
        
        // Assert
        Assert.Equivalent(expected, result);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GetAppDefinition__ThrowsException__WhenTheAppEntryIsEmpty(string appEntryName)
    {
        // Arrange
        var parser = new AppEntryParser(_configPaths, _jsonSerializerOptions, _logger);
        
        // Act
        var exception = Record.Exception(() => parser.GetAppEntry(appEntryName));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal("The app entry name is invalid!", exception.Message);
    }
    
    [Fact]
    public void GetFriendlyNames__ReturnsFriendlyNames__WhenTheyExist()
    {
        // Arrange
        var parser = new AppEntryParser(_configPaths, _jsonSerializerOptions, _logger);
        var expected = new[] { "mpv (Play)" };
        
        // Act
        var result = parser.GetFriendlyNames(new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play" ]
        });
        
        // Assert
        Assert.Equivalent(expected, result);
    }
}
