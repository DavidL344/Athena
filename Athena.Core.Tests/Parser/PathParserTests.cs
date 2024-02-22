using System.Reflection;
using Athena.Core.Parser;
using Athena.Core.Parser.Options;
using Xunit.Abstractions;

namespace Athena.Core.Tests.Parser;

public class PathParserTests
{
    private readonly string _workingDir = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))!;
    private readonly ITestOutputHelper _console;

    public PathParserTests(ITestOutputHelper testOutputHelper)
    {
        _console = testOutputHelper;
    }

    [Theory]
    [InlineData("file.txt")]
    [InlineData("./file.txt")]
    [InlineData("../file.txt")]
    public void GetPath__ReturnsPath__WhenFilePathIsRelative(string filePath)
    {
        // Arrange
        var options = new PathParserOptions { OpenLocally = true };
        var parser = new PathParser(options);
        var expected = new Uri(Path.GetFullPath(filePath));
        
        // Act
        var result = parser.GetUri(filePath);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("~/file.txt")]
    public void GetPath__ReturnsPath__WhenFilePathHasEnvironmentVariables(string filePath)
    {
        // Arrange
        var options = new PathParserOptions { OpenLocally = true };
        var parser = new PathParser(options);
        filePath = Environment.ExpandEnvironmentVariables(filePath
            .Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
        var expected = new Uri(filePath);
        
        // Act
        var result = parser.GetUri(filePath);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("C:/file.txt")]
    [InlineData("D:/file.txt")]
    [InlineData("/file.txt")]
    [InlineData("/home/user/file.txt")]
    [InlineData(@"C:\file.txt")]
    [InlineData(@"D:\file.txt")]
    [InlineData(@"\file.txt")]
    [InlineData(@"\home\user\file.txt")]
    public void GetPath__ReturnsPath__WhenFilePathIsAbsolute(string filePath)
    {
        // Arrange
        var options = new PathParserOptions { OpenLocally = true };
        var parser = new PathParser(options);
        var expected = new Uri(filePath
            .Replace("/", Path.DirectorySeparatorChar.ToString())
            .Replace(@"\", Path.DirectorySeparatorChar.ToString()));
        
        // Act
        var result = parser.GetUri(filePath);
        _console.WriteLine(expected.ToString());
        _console.WriteLine(result.ToString());
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("C:/./file.txt")]
    [InlineData("D:/./test/../file.txt")]
    [InlineData("C:/test/../file.txt")]
    [InlineData("D:/test/./file.txt")]
    [InlineData("/./home/./user/../user/./file.txt")]
    [InlineData(@"C:\.\file.txt")]
    [InlineData(@"D:\.\test\..\file.txt")]
    [InlineData(@"C:\test\..\file.txt")]
    [InlineData(@"D:\test\.\file.txt")]
    [InlineData(@"\home\user\file.txt")]
    [InlineData(@"\.\home\.\user\../user/./file.txt")]
    
    public void GetPath__ReturnsPath__WhenFilePathIsSemiAbsolute(string filePath)
    {
        GetPath__ReturnsPath__WhenFilePathIsAbsolute(filePath);
    }
    
    [Theory]
    [InlineData("http://example.com/file.txt")]
    [InlineData("https://example.com/file.txt")]
    [InlineData("ftp://example.com/file.txt")]
    [InlineData("sftp://example.com/file.txt")]
    public void GetPath__ReturnsPath__WhenFilePathIsRemote(string filePath)
    {
        // Arrange
        var options = new PathParserOptions { OpenLocally = false };
        var parser = new PathParser(options);
        var expected = new Uri(filePath);
        
        // Act
        var result = parser.GetUri(filePath);
        
        // Assert
        Assert.Equal(expected, result);
    }
}
