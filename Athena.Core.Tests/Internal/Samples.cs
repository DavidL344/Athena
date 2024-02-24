using Athena.Core.Startup;

namespace Athena.Core.Tests.Internal;

public class Samples
{
    public static async Task Generate(string appDataDir)
    {
        Remove(appDataDir);
        await Checks.CheckConfiguration(appDataDir);
    }
    
    public static void Remove(string userDir)
    {
        if (Directory.Exists(userDir))
            Directory.Delete(userDir, true);
    }
}
