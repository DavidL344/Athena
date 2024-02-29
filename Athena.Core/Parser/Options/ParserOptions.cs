namespace Athena.Core.Parser.Options;

public class ParserOptions
{
    public bool OpenLocally { get; set; }
    public string[] StreamableProtocolPrefixes { get; set; } = ["athena", "stream"];
    public bool AllowProtocols { get; set; }
}
