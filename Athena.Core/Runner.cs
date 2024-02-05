using Athena.Core.Model;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;

namespace Athena.Core;

public class Runner
{
    private readonly ILogger<Runner> _logger;

    public Runner(ILogger<Runner> logger)
    {
        _logger = logger;
    }
    
    public async Task<int> Run(string executablePath, string arguments, ConsolePipes? consolePipes = null)
    {
        _logger.LogInformation(
            "Opening {Path} with params {Arguments}...",
            executablePath, arguments);
        
        consolePipes ??= new ConsolePipes();
        var result = await Cli.Wrap(executablePath)
            .WithArguments(arguments)
            .WithStandardInputPipe(consolePipes.StdIn)
            .WithStandardOutputPipe(consolePipes.StdOut)
            .WithStandardErrorPipe(consolePipes.StdErr)
            .ExecuteBufferedAsync();

        var exitCode = result.ExitCode;
        _logger.LogInformation("Process exited with exit code {ExitCode}", exitCode);
        
        return exitCode;
    }
}
