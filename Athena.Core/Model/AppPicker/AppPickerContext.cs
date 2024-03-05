namespace Athena.Core.Model.AppPicker;

public class AppPickerContext
{
    public string FriendlyName { get; init; } = default!;
    public string FilePath { get; init; } = default!;
    public string[] AppEntries { get; init; } = default!;
}
