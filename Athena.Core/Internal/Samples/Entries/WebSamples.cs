using Athena.Core.Model;

namespace Athena.Core.Internal.Samples.Entries;

internal class WebSamples : ISample
{
    public Dictionary<string, AppEntry> Entries { get; } = new()
    {
        {
            "firefox.open", new AppEntry
            {
                Name = "Firefox (Open)",
                Path = "firefox",
                Arguments = "$URL"
            }
        },
        {
            "firefox.open-private", new AppEntry
            {
                Name = "Firefox (Private)",
                Path = "firefox",
                Arguments = "--private-window $URL"
            }
        },
        {
            "chromium.open", new AppEntry
            {
                Name = "Chromium (Open)",
                Path = "chromium",
                Arguments = "$URL"
            }
        },
        {
            "chromium.open-private", new AppEntry
            {
                Name = "Chromium (Private)",
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

    public List<string> MimeTypes { get; } =
    [
        "text/html", // html
        "x-scheme-handler/http", // http
        "x-scheme-handler/https" // https
    ];
}
