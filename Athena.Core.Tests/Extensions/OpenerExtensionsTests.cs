using Athena.Core.Extensions;
using Athena.Core.Model;

namespace Athena.Core.Tests.Extensions;

public class OpenerExtensionsTests
{
    [Fact]
    public void GetAppEntryIndex__ReturnsTheCorrectIndex__WhenEntryNameIsSpecified()
    {
        // Arrange
        const int expectedIndex = 1;
        var fileExtension = new FileExtension
        {
            Name = "Text file",
            AppList =
            [
                "vi.open",
                "notepad.open", // The expected index is 1
                "nano.open"
            ]
        };
        
        // Act
        var result = fileExtension.GetAppEntryIndex("notepad.open");
        
        // Assert
        Assert.Equal(expectedIndex, result);
    }
}
