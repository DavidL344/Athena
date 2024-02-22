using Athena.CoreOld.Model.Entry;
using Athena.CoreOld.Model.Opener;

namespace Athena.CoreOld.Internal.Samples;

public class ArchiveSamples : ISample
{
    public Dictionary<string, AppEntry> Entries { get; } = new()
    {
        {
            "unzip.extract", new AppEntry
            {
                Name = "Unzip (Extract)",
                Type = EntryType.File,
                Path = "unzip",
                Arguments = "$FILE"
            }
        },
        {
            "unzip.extract-to-desktop", new AppEntry
            {
                Name = "Unzip (Extract to Desktop)",
                Type = EntryType.File,
                Path = "unzip",
                Arguments = "-d ~/Desktop $FILE"
            }
        },
        {
            "unzip.list", new AppEntry
            {
                Name = "Unzip (List)",
                Type = EntryType.File,
                Path = "unzip",
                Arguments = "-l $FILE"
            }
        }
    };
    
    public Dictionary<string, FileExtension> FileExtensions { get; } = new()
    {
        {
            "zip", new FileExtension
            {
                Name = "Zip Archive",
                AppList =
                [
                    "unzip.extract",
                    "unzip.extract-to-desktop",
                    "unzip.list"
                ]
            }
        },
        {
            "7z", new FileExtension
            {
                Name = "7-Zip Archive",
                AppList =
                [
                    "unzip.extract",
                    "unzip.extract-to-desktop",
                    "unzip.list"
                ]
            }
        }
    };
    
    public Dictionary<string, Protocol> Protocols { get; } = new();
}
