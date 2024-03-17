using Athena.Core.Internal.Helpers;
using Athena.Core.Options;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;

namespace Athena.Core.Runner;

public class AppRunner
{
    private readonly RunnerOptions _options;
    private readonly ILogger<AppRunner> _logger;
    
    public AppRunner(RunnerOptions options, ILogger<AppRunner> logger)
    {
        _options = options;
        _logger = logger;
    }
    
    public async Task<int> RunAsync(string executablePath, string arguments)
    {
        if (!File.Exists(executablePath) && !File.Exists(SystemHelper.WhereIs(executablePath)))
            throw new ApplicationException($"Command '{executablePath}' not found!");
        
        _logger.LogDebug(
            "Opening {Path} with params {Arguments}...",
            executablePath, arguments);
        
        var result = await Cli.Wrap(executablePath)
            .WithArguments(arguments)
            .WithStandardInputPipe(_options.StdIn)
            .WithStandardOutputPipe(_options.StdOut)
            .WithStandardErrorPipe(_options.StdErr)
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();
        
        var exitCode = result.ExitCode;
        _logger.LogDebug("Process exited with exit code {ExitCode}", exitCode);
        
        return exitCode;
    }
}
