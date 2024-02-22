namespace Athena.CoreOld.Model.Configuration;

public class Config
{
    public bool EnableProtocolHandler { get; set; }
    public string[] StreamableProtocolPrefixes { get; set; } = ["athena", "stream"];
}
