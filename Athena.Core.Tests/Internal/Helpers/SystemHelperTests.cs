using Athena.Core.Internal.Helpers;

namespace Athena.Core.Tests.Internal.Helpers;

public class SystemHelperTests
{
    [Fact]
    public void IsInPath__ReturnsTrue__WhenCommandIsInPath()
    {
        // Arrange
        var command = "dotnet";
        
        // Act
        var result = SystemHelper.IsInPath(command);
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInPath__ReturnsFalse__WhenCommandIsNotInPath()
    {
        // Arrange
        var command = "not-a-real-command";

        // Act
        var result = SystemHelper.IsInPath(command);

        // Assert
        Assert.False(result);
    }
}
