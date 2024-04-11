using System.Reflection;
using System.Resources;
using System.Text;

namespace Athena.Resources;

public static class ResourceLoader
{
    public static string Load(string resourceName)
    {
        var resourcePath = $"Athena.Resources.{resourceName.Replace('/', '.')}";
        var assembly = Assembly.GetExecutingAssembly();
        
        using var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            throw new MissingManifestResourceException(
                $"Resource {resourcePath} not found in {assembly.FullName}");
        }
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
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
}
