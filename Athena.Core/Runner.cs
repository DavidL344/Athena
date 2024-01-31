using Athena.Core.Model;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;

namespace Athena.Core;

public class Runner(ILogger logger)
{
    public async Task<int> Run(string executablePath, string arguments, ConsolePipes? consolePipes = null)
    {
        logger.LogInformation(
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
        logger.LogInformation("Process exited with exit code {ExitCode}", exitCode);
        
        return exitCode;
    }
}
