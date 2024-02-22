using System.Text.Json;
using Athena.Core.Model.Opener;
using Athena.Core.Parser;
using Athena.Core.Parser.Options;
using Athena.Core.Parser.Shared;

namespace Athena.Core.Tests.Parser;

public class AppParserTests
{
    [Theory]
    [InlineData("/file.mp4", true)]
    [InlineData("/file.mp4", false)]
    public async Task GetAppDefinition__ReturnsAppDefinition__WhenItExists(string filePath, bool expandVars)
    {
        // Arrange
        var options = new ParserOptions();
        var parser = new AppParser(options);
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
        var parser = new AppParser(options);
        
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
        var parser = new AppParser(options);
        
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
        var parser = new AppParser(options);
        
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
        var parser = new AppParser(options);
        var expected = new AppEntry
        {
            Name = "mpv (Play)",
            Path = "mpv",
            Arguments = "$FILE",
            RemoveProtocol = removeProtocol
        };
        
        // Act
        var filePath = Path.Combine(Vars.ConfigPaths[ConfigType.Entries], ".temp.open.json");
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
        var parser = new AppParser(options);
        
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
