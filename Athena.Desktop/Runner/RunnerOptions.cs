using CliWrap;

namespace Athena.Desktop.Runner;

public class RunnerOptions
{
    public PipeSource StdIn { get; init; } = PipeSource.FromStream(Console.OpenStandardInput());
    public PipeTarget StdOut { get; init; } = PipeTarget.ToStream(Console.OpenStandardOutput());
    public PipeTarget StdErr { get; init; } = PipeTarget.ToStream(Console.OpenStandardError());
}
