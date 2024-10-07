using Athena.Core.Internal.Helpers;

namespace Athena.Core.Tests.Helpers;

public class ParserHelperTests
{
    [Theory]
    [InlineData("file:///file.mp4")]
    [InlineData("file://file.mp4")]
    public void ParseStreamPath__RemovesFileProtocol__WhenItExists(string filePath)
    {
        // Arrange
        var expected = Path.GetFullPath(filePath.Split("://").LastOrDefault()!);
        
        // Act
        var result = ParserHelper.ParseStreamPath(filePath, []);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("athena:https://example.com/file.mp4")]
    [InlineData("stream:https://example.com/file.mp4")]
    public void ParseStreamPath__RemovesFileProtocol__WhenProtocolIsStreamable(string filePath)
    {
        // Arrange
        var expected = string.Join(null, filePath.Substring(filePath.Split(":").FirstOrDefault()!.Length + 1));
        
        // Act
        var result = ParserHelper.ParseStreamPath(filePath, ["athena", "stream"]);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("$1")]
    [InlineData("$TEST")]
    [InlineData("$1TEST")]
    [InlineData("$TEST1")]
    public void ExpandEnvironmentVariables__ConvertsUnixFormat__WhenItIsPresent(string variable)
    {
        // Arrange
        var expected = variable.Replace('$', '%');
        if (!int.TryParse(expected.Substring(1), out _)) expected += "%";
        
        // Act
        var result = ParserHelper.ExpandEnvironmentVariables(variable);
        
        // Assert
        Assert.Equal(expected, result);
    }
}
