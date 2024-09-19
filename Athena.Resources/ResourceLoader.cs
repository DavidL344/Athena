using System.Reflection;
using System.Resources;
using System.Text;

namespace Athena.Resources;

public static class ResourceLoader
{
    public static string Load(string resourceName)
    {
        var stream = LoadResource(resourceName);
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static void Save(string resourceName, string location)
    {
        var stream = LoadResource(resourceName);

        using var file = File.Create(location);
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(file);
    }
    
    public static string GetResourceNamesAsString()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();
        
        var sb = new StringBuilder();
        foreach (var resourceName in resourceNames)
        {
            sb.AppendLine(resourceName);
        }
        
        return sb.ToString();
    }

    private static Stream LoadResource(string resourceName)
    {
        var resourcePath = $"Athena.Resources.{resourceName.Replace('/', '.')}";
        var assembly = Assembly.GetExecutingAssembly();
        
        var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            throw new MissingManifestResourceException(
                $"Resource {resourcePath} not found in {assembly.FullName}");
        }
        
        return stream;
    }
}
