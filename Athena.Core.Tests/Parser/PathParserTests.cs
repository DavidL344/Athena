using System.Reflection;
using Athena.Core.Parser;
using Athena.Core.Parser.Options;
using Xunit.Abstractions;

namespace Athena.Core.Tests.Parser;

public class PathParserTests : IDisposable
{
    private readonly string _workingDir;
    private readonly ITestOutputHelper _console;
    private readonly string _testsConfigDir;

    public PathParserTests(ITestOutputHelper testOutputHelper)
    {
        _workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        _console = testOutputHelper;
        _testsConfigDir = Path.Combine(_workingDir, "user-path-parser-tests");
        
        Internal.Samples.Generate(_testsConfigDir).GetAwaiter().GetResult();
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Internal.Samples.Remove(_testsConfigDir);
    }

    [Theory]
    [InlineData("file.txt")]
    [InlineData("./file.txt")]
    [InlineData("../file.txt")]
    public void GetPath__ReturnsPath__WhenFilePathIsRelative(string filePath)
    {
        // Arrange
        var options = new ParserOptions { OpenLocally = true };
        var parser = new PathParser(options);
        var expected = new Uri(Path.GetFullPath(filePath));
        
        // Act
        var result = parser.GetUri(filePath);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("~/file.txt")]
    [InlineData("$HOME/file.txt")]
    [InlineData("%HOME%/file.txt")]
    [InlineData("$CURRENT_DIR/file.txt")]
    [InlineData("%CURRENT_DIR%/file.txt")]
    public void GetPath__ReturnsPath__WhenFilePathHasEnvironmentVariables(string filePath)
    {
        // Arrange
        var options = new ParserOptions { OpenLocally = true };
        var parser = new PathParser(options);
        
        Environment.SetEnvironmentVariable("CURRENT_DIR", _workingDir);
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            filePath = filePath
                .Replace("$HOME", "$USERPROFILE")
                .Replace("%HOME%", "%USERPROFILE%");
        
        filePath = Environment.ExpandEnvironmentVariables(filePath
            .Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            .Replace("$HOME", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            .Replace("$CURRENT_DIR", _workingDir));
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
        var options = new ParserOptions { OpenLocally = true };
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
        var options = new ParserOptions { OpenLocally = false };
        var parser = new PathParser(options);
        var expected = new Uri(filePath);
        
        // Act
        var result = parser.GetUri(filePath);
        
        // Assert
        Assert.Equal(expected, result);
    }
}
