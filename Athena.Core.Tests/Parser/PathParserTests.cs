using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athena.Core.Parser;
using Athena.Core.Parser.Options;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Athena.Core.Tests.Parser;

public class PathParserTests : IDisposable
{
    private readonly string _workingDir;
    private readonly ITestOutputHelper _console;
    private readonly string _testsConfigDir;
    private readonly ILogger<PathParser> _logger;

    public PathParserTests(ITestOutputHelper testOutputHelper)
    {
        _workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        _console = testOutputHelper;
        _testsConfigDir = Path.Combine(_workingDir, "user-path-parser-tests");
        _logger = new Logger<PathParser>(new LoggerFactory());
        
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = false,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        Internal.Samples.Generate(_testsConfigDir, jsonSerializerOptions).GetAwaiter().GetResult();
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
        var parser = new PathParser(_logger);
        var expected = Path.GetFullPath(filePath);
        
        // Act
        var result = parser.GetPath(filePath, options);
        
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
        var parser = new PathParser(_logger);
        
        Environment.SetEnvironmentVariable("CURRENT_DIR", _workingDir);
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            filePath = filePath
                .Replace("$HOME", "$USERPROFILE")
                .Replace("%HOME%", "%USERPROFILE%");
        
        filePath = Environment.ExpandEnvironmentVariables(filePath
            .Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            .Replace("$HOME", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            .Replace("$CURRENT_DIR", _workingDir));
        var expected = filePath;
        
        // Act
        var result = parser.GetPath(filePath, options);
        
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
        var parser = new PathParser(_logger);
        var expected = filePath.StartsWith('\\')
                ? $"{Path.GetPathRoot(Directory.GetCurrentDirectory())}{filePath
                    .Remove(0, 1).Replace('\\', Path.DirectorySeparatorChar)}"
                : filePath;
        
        // Act
        var result = parser.GetPath(filePath, options);
        _console.WriteLine(expected);
        _console.WriteLine(result);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("C:/./file.txt")]
    [InlineData("D:/./test/../file.txt")]
    [InlineData("C:/test/../file.txt")]
    [InlineData("D:/test/./file.txt")]
    [InlineData(@"C:\.\file.txt")]
    [InlineData(@"D:\.\test\..\file.txt")]
    [InlineData(@"C:\test\..\file.txt")]
    [InlineData(@"D:\test\.\file.txt")]
    [InlineData(@"\home\user\file.txt")]
    public void GetPath__ReturnsPath__WhenFilePathIsSemiAbsolute(string filePath)
    {
        GetPath__ReturnsPath__WhenFilePathIsAbsolute(filePath);
    }
    
    [Theory]
    [InlineData("/./home/./user/../user/./file.txt")]
    [InlineData(@"\.\home\.\user\../user/./file.txt")]
    public void GetPath__ReturnsPath__WhenFilePathHasMultipleDirectorySeparators(string filePath)
    {
        // Arrange
        var options = new ParserOptions { OpenLocally = true };
        var parser = new PathParser(_logger);
        var expected = Path.GetFullPath(filePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace($"..{Path.DirectorySeparatorChar}", "<up>")
            .Replace($".{Path.DirectorySeparatorChar}", "")
            .Replace("<up>", $"..{Path.DirectorySeparatorChar}"));
        
        // Act
        var result = parser.GetPath(filePath, options);
        
        // Assert
        Assert.Equal(expected, result);
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
        var parser = new PathParser(_logger);
        var expected = filePath;
        
        // Act
        var result = parser.GetPath(filePath, options);
        
        // Assert
        Assert.Equal(expected, result);
    }
}
