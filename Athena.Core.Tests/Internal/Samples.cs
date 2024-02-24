using System.Text.Json;
using Athena.Core.Startup;

namespace Athena.Core.Tests.Internal;

public class Samples
{
    public static async Task Generate(string appDataDir, JsonSerializerOptions serializerOptions)
    {
        Remove(appDataDir);
        await Checks.CheckConfiguration(appDataDir, serializerOptions);
    }
    
    public static void Remove(string userDir)
    {
        if (Directory.Exists(userDir))
            Directory.Delete(userDir, true);
    }
}
