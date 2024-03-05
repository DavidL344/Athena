namespace Athena.Cli.Commands.Internal.Model;

public record OpenFileOptions
{
    public string FilePath { get; set; } = default!;
    public string? EntryName { get; set; }
    public int? EntryId { get; set; }
    public bool OpenWithTheFirstEntry { get; set; }
    public bool OpenLocally { get; set; }
    public bool ForceShowAppPicker { get; set; }
}
