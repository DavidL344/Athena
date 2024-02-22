using Athena.CoreOld.Model.Entry;
using Athena.CoreOld.Model.Opener;

namespace Athena.CoreOld.Internal.Samples;

public class MediaSamples : ISample
{
    public Dictionary<string, AppEntry> Entries { get; } = new()
    {
        {
            "mpv.play", new AppEntry
            {
                Name = "mpv (Play)",
                Type = EntryType.All,
                Path = "mpv",
                Arguments = "$FILE"
            }
        },
        {
            "vlc.play", new AppEntry
            {
                Name = "VLC Media Player (Play)",
                Type = EntryType.All,
                Path = "vlc",
                Arguments = "$FILE"
            }
        }
    };
    
    public Dictionary<string, FileExtension> FileExtensions { get; } = new()
    {
        {
            "mp4", new FileExtension
            {
                Name = "MP4 Video",
                AppList =
                [
                    "mpv.play",
                    "vlc.play"
                ]
            }
        },
        {
            "mkv", new FileExtension
            {
                Name = "MKV Video",
                AppList =
                [
                    "mpv.play",
                    "vlc.play"
                ]
            }
        },
        {
            "avi", new FileExtension
            {
                Name = "AVI Video",
                AppList =
                [
                    "mpv.play",
                    "vlc.play"
                ]
            }
        },
        {
            "mp3", new FileExtension
            {
                Name = "MP3 Audio",
                AppList =
                [
                    "mpv.play",
                    "vlc.play"
                ]
            }
        },
        {
            "flac", new FileExtension
            {
                Name = "FLAC Audio",
                AppList =
                [
                    "mpv.play",
                    "vlc.play"
                ]
            }
        }
    };
    
    public Dictionary<string, Protocol> Protocols { get; } = new();
}
