using Athena.Core.Model.Entry;
using Athena.Core.Model.Opener;

namespace Athena.Core.Internal.Samples;

public class WebSamples : ISample
{
    public Dictionary<string, AppEntry> Entries { get; } = new()
    {
        {
            "athena.protocol", new AppEntry
            {
                Name = "Athena",
                Type = EntryType.Protocol,
#if DEBUG
                Path = Path.Combine(Vars.AssemblyDir, "Athena"),
#else
                Path = "athena",
#endif
                Arguments = "run $URL -l",
                RemoveProtocol = true
            }
        },
        {
            "firefox.open", new AppEntry
            {
                Name = "Firefox (Open)",
                Type = EntryType.All,
                Path = "firefox",
                Arguments = "$URL"
            }
        },
        {
            "firefox.open-private", new AppEntry
            {
                Name = "Firefox (Private)",
                Type = EntryType.All,
                Path = "firefox",
                Arguments = "--private-window $URL"
            }
        },
        {
            "chromium.open", new AppEntry
            {
                Name = "Chromium (Open)",
                Type = EntryType.All,
                Path = "chromium",
                Arguments = "$URL"
            }
        },
        {
            "chromium.open-private", new AppEntry
            {
                Name = "Chromium (Private)",
                Type = EntryType.All,
                Path = "chromium",
                Arguments = "--incognito $URL"
            }
        }
    };

    public Dictionary<string, FileExtension> FileExtensions { get; } = new()
    {
        {
            "html", new FileExtension
            {
                Name = "HTML file",
                AppList =
                [
                    "firefox.open",
                    "firefox.open-private",
                    "chromium.open",
                    "chromium.open-private"
                ]
            }
        }
    };
    
    public Dictionary<string, Protocol> Protocols { get; } = new()
    {
        {
            "athena", new Protocol
            {
                Name = "athena",
                AppList =
                [
                    "athena.protocol"
                ],
                DefaultApp = "athena.protocol"
            }
        },
        {
            "http", new Protocol
            {
                Name = "http",
                AppList =
                [
                    "firefox.open",
                    "firefox.open-private",
                    "chromium.open",
                    "chromium.open-private"
                ]
            }
        },
        {
            "https", new Protocol
            {
                Name = "https",
                AppList =
                [
                    "firefox.open",
                    "firefox.open-private",
                    "chromium.open",
                    "chromium.open-private"
                ]
            }
        }
    };
}
