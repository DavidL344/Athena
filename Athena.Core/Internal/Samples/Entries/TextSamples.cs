using Athena.Core.Model;

namespace Athena.Core.Internal.Samples.Entries;

internal class TextSamples : ISample
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
        {
            "codium.open", new AppEntry
            {
                Name = "Visual Studio Codium (Open)",
                Path = "codium",
                Arguments = "$FILE"
            }
        },
        {
            "code.open", new AppEntry
            {
                Name = "Visual Studio Code (Open)",
                Path = "code",
                Arguments = "$FILE"
            }
        },
        {
            "notepad.open", new AppEntry
            {
                Name = "Notepad (Open)",
                Path = "notepad",
                Arguments = "$FILE"
            }
        }
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
                    "gedit.open",
                    "codium.open",
                    "code.open",
                    "notepad.open"
                ]
            }
        }
    };
    
    public Dictionary<string, Protocol> Protocols { get; } = new();
}
