using System.Text.Json.Serialization;

namespace Athena.Model;

public class FileExtension
{
    [JsonRequired]
    public required string Name { get; set; }
    
    [JsonRequired]
    public required string[] AppList { get; set; }
}
