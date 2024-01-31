using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athena.Core.Model;

namespace Athena.Core.Internal;

public class Vars
{
    public static string AppDataDir
    {
        get
        {
            var userConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "athena");
            var portableConfigDir = Path.Combine(AssemblyDir, "user");
            return Directory.Exists(portableConfigDir) ? portableConfigDir : userConfigDir;
        }
    }

    public static readonly Dictionary<ConfigType, string> ConfigPaths = new()
    {
        { ConfigType.Entries, Path.Combine(AppDataDir, "entries") },
        { ConfigType.Files, Path.Combine(AppDataDir, "files") },
        { ConfigType.Protocols, Path.Combine(AppDataDir, "protocols") }
    };
    
    public static string AssemblyDir
        => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    
    public static JsonSerializerOptions JsonSerializerOptions => new() 
    {
        AllowTrailingCommas = false,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
