using Athena.Core.Model.Opener;

namespace Athena.Core.Samples;

public class TextSamples : ISample
{
    public Dictionary<string, AppEntry> Entries { get; } = new()
    {
        {
            "vi.open", new AppEntry
            {
                Name = "Vi (Open)",
                Path = "vi",
                Arguments = "$FILE"
            }
        },
        {
            "nano.open", new AppEntry
            {
                Name = "nano (Open)",
                Path = "nano",
                Arguments = "$FILE"
            }
        },
        {
            "gedit.open", new AppEntry
            {
                Name = "GNOME Editor (Open)",
                Path = "gedit",
                Arguments = "$FILE"
            }
        },
    };
    
    public Dictionary<string, FileExtension> FileExtensions { get; } = new()
    {
        {
            "txt", new FileExtension
            {
                Name = "Text file",
                AppList =
                [
                    "vi.open",
                    "nano.play",
                    "gedit.open"
                ]
            }
        }
    };
    
    public Dictionary<string, Protocol> Protocols { get; } = new();
}
