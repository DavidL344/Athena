namespace Athena.Core.Configuration;

public record Config
{
    public bool EnableProtocolHandler { get; set; }
    public string[] StreamableProtocolPrefixes { get; set; } = ["athena", "stream"];
    public string Editor { get; set; } = OperatingSystem.IsWindows() ? "notepad" : "gedit";
    public string Version { get; set; } = "0.0.0";
}
