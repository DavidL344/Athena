using System.Reflection;
using System.Text.Json;
using Athena.Core.Model.Opener;
using Athena.Core.Parser;
using Athena.Core.Parser.Options;
using Athena.Core.Parser.Shared;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Tests.Parser;

public class AppParserTests : IDisposable
{
    private readonly string _testsConfigDir;
    private readonly Dictionary<ConfigType, string> _configPaths;
    private readonly Logger<AppParser> _logger;

    public AppParserTests()
    {
        var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        
        _testsConfigDir = Path.Combine(workingDir, "user-app-parser-tests");
        _configPaths = new Dictionary<ConfigType, string>
        {
            { ConfigType.Entries, Path.Combine(_testsConfigDir, "entries") },
            { ConfigType.Files, Path.Combine(_testsConfigDir, "files") },
            { ConfigType.Protocols, Path.Combine(_testsConfigDir, "protocols") }
        };
        _logger = new Logger<AppParser>(new LoggerFactory());
        
        Internal.Samples.Generate(_testsConfigDir).GetAwaiter().GetResult();
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Internal.Samples.Remove(_testsConfigDir);
    }
    
    [Theory]
    [InlineData("/file.mp4", true)]
    [InlineData("/file.mp4", false)]
    public async Task GetAppDefinition__ReturnsAppDefinition__WhenItExists(string filePath, bool expandVars)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new AppParser(_configPaths, options, _logger);
        var expected = new AppEntry
        {
            Name = "mpv (Play)",
            Path = "mpv",
            Arguments = expandVars ? $"\"{filePath}\"" : "$FILE"
        };
        
        // Act
        var result = await parser.GetAppDefinition(new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play" ]
        }, filePath, "mpv.play", expandVars);
        
        // Assert
        Assert.Equivalent(expected, result);
    }
    
    [Theory]
    [InlineData("/file.mp4", "mpv.pause")]
    public async Task GetAppDefinition__ThrowsException__WhenItDoesNotExist(string filePath, string entryName)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new AppParser(_configPaths, options, _logger);
        
        // Act
        var exception = await Record.ExceptionAsync(() => parser.GetAppDefinition(new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play" ]
        }, filePath, entryName, expandVars: false));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal($"The app entry ({entryName}) isn't registered with the opener (MP4 Video)!",
            exception.Message);
    }
    
    [Theory]
    [InlineData("/file.mp4", -1)]
    [InlineData("/file.mp4", 1)]
    public async Task GetAppDefinition__ThrowsException__WhenIndexIsOutOfRange(string filePath, int entryIndex)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new AppParser(_configPaths, options, _logger);
        
        // Act
        var exception = await Record.ExceptionAsync(() => parser.GetAppDefinition(new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play" ]
        }, filePath, entryIndex, expandVars: false));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal("The entry ID is out of range!", exception.Message);
    }
    
    [Theory]
    [InlineData("/file.random", "random.play", true)]
    [InlineData("/file.random", "random.open", false)]
    public async Task GetAppDefinition__ThrowsException__WhenAppDefinitionDoesNotExist(
        string filePath, string definitionName, bool expandVars)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new AppParser(_configPaths, options, _logger);
        
        // Act
        var exception = await Record.ExceptionAsync(() => parser.GetAppDefinition(new FileExtension
        {
            Name = "Random File",
            AppList = [ definitionName ]
        }, filePath, definitionName, expandVars));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal($"The entry ({definitionName}) isn't defined!", exception.Message);
    }

    [Theory]
    [InlineData("https://example.com/file.temp", true)]
    [InlineData("https://example.com/file.temp", false)]
    public async Task GetAppDefinition__RemovesProtocol__WhenRequested(string url, bool removeProtocol)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new AppParser(_configPaths, options, _logger);
        var expected = new AppEntry
        {
            Name = "mpv (Play)",
            Path = "mpv",
            Arguments = "$FILE",
            RemoveProtocol = removeProtocol
        };
        
        // Act
        var filePath = Path.Combine(_configPaths[ConfigType.Entries], ".temp.open.json");
        var fileContents = JsonSerializer.Serialize(expected, Vars.JsonSerializerOptions);
        await File.WriteAllTextAsync(filePath, fileContents);
        
        var result = await parser.GetAppDefinition(new FileExtension
        {
            Name = "A temporary file",
            AppList = [ ".temp.open" ]
        }, url, ".temp.open", expandVars: false);
        File.Delete(filePath);
        
        // Assert
        Assert.Equivalent(expected, result);
    }
    
    [Fact]
    public async Task GetAppDefinition__ThrowsException__WhenThePathIsEmpty()
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new AppParser(_configPaths, options, _logger);
        
        // Act
        var exception = await Record.ExceptionAsync(() => parser.GetAppDefinition(new FileExtension
        {
            Name = "MP4 Video",
            AppList = [ "mpv.play" ]
        }, string.Empty, "mpv.play", expandVars: false));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal("The file path is empty!", exception.Message);
    }
}
