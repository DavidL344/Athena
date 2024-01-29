using System.Text.Json;
using System.Text.Json.Serialization;

namespace Athena.Internal;

public class Vars
{
    public static string AppDataDir
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "athena");
    
    public static JsonSerializerOptions JsonSerializerOptions => new() 
    {
        AllowTrailingCommas = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
