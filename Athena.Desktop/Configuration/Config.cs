using Athena.Desktop.Helpers;

namespace Athena.Desktop.Configuration;

public record Config
{
    public bool EnableProtocolHandler { get; set; }
    public string[] StreamableProtocolPrefixes { get; set; }
    public string Editor { get; set; }
    public string Version { get; set; }

    public Config()
    {
        EnableProtocolHandler = false;
        StreamableProtocolPrefixes = ["athena", "stream"];
        Editor = SystemHelper.GetDefaultEditor();
        Version = $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}";
    }
}
