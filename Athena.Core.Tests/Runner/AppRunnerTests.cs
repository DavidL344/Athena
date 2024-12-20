using System.Reflection;
using System.Text;
using Athena.Desktop.Runner;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Tests.Runner;

public class AppRunnerTests
{
    private readonly string[] _builtinCommands = ["echo"];
    private readonly StringBuilder _stdOutBuffer;
    private readonly StringBuilder _stdErrBuffer;
    private readonly AppRunner _runner;

    public AppRunnerTests()
    {
        _stdOutBuffer = new StringBuilder();
        _stdErrBuffer = new StringBuilder();
        
        var options = new RunnerOptions
        {
            StdOut = PipeTarget.ToStringBuilder(_stdOutBuffer),
            StdErr = PipeTarget.ToStringBuilder(_stdErrBuffer)
        };
        
        var logger = new Logger<AppRunner>(new LoggerFactory());
        
        _runner = new AppRunner(options, logger);
    }

    private void ClearBuffers()
    {
        _stdOutBuffer.Clear();
        _stdErrBuffer.Clear();
    }
    
    [SkippableTheory]
    [InlineData("echo", "Hello, World!", "Hello, World!")]
    public async Task Run__RunsCommand__WhenItExists(string executablePath, string arguments, string expectedStdOut)
    {
        // Arrange
        Skip.If(IsSkippable(executablePath));
        ClearBuffers();
        
        // Act
        var exitCode = await _runner.RunAsync(executablePath, arguments);
        
        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal(expectedStdOut, _stdOutBuffer.ToString().Trim());
        Assert.Empty(_stdErrBuffer.ToString().Trim());
    }
    
    [SkippableTheory]
    [InlineData("dotnet", "--version")]
    [InlineData("dotnet", "--help")]
    [InlineData("echo", "Hello, World!")]
    public async Task Run__ReturnsCorrectOutput__WhenExecutionFinishes(string executablePath, string arguments)
    {
        // Arrange
        Skip.If(IsSkippable(executablePath));
        ClearBuffers();
        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();
        
        // Act
        var actualExitCode = await _runner.RunAsync(executablePath, arguments);
        var result = await Cli.Wrap(executablePath)
            .WithArguments(arguments)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOut))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErr))
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();
        var expectedExitCode = result.ExitCode;
        
        // Assert
        Assert.Equal(expectedExitCode, actualExitCode);
        Assert.Equal(stdOut.ToString(), _stdOutBuffer.ToString());
        Assert.Equal(stdErr.ToString(), _stdErrBuffer.ToString());
    }

    [Fact]
    public async Task Run__RunsExecutable__WhenItExists()
    {
        // Arrange
        ClearBuffers();
        var executablePath = Assembly.GetAssembly(typeof(ITestsMarker))!.Location;
        
        // Act
        var exitCode = await _runner.RunAsync("dotnet", executablePath);
        
        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal(0, _stdOutBuffer.Length);
        Assert.Equal(0, _stdErrBuffer.Length);
    }
    
    [Theory]
    [InlineData("nonexistent", "application")]
    [InlineData("/nonexistent", "application")]
    public async Task Run__ThrowsException__WhenItDoesNotExist(string executablePath, string arguments)
    {
        // Arrange
        ClearBuffers();
        
        // Act
        var exception = await Record.ExceptionAsync(
            async () => await _runner.RunAsync(executablePath, arguments));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ApplicationException>(exception);
        Assert.Equal($"Command '{executablePath}' not found!", exception.Message);
        Assert.Empty(_stdOutBuffer.ToString().Trim());
        Assert.Empty(_stdErrBuffer.ToString().Trim());
    }

    private bool IsSkippable(string commandName)
        => OperatingSystem.IsWindows() && _builtinCommands.Contains(commandName);
}
