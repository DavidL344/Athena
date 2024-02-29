namespace Athena.Core.Model;

public record Config
{
    public bool EnableProtocolHandler { get; set; }
    public string[] StreamableProtocolPrefixes { get; set; } = ["athena", "stream"];

    public string Version { get; set; } = "0.0.0";
}
