using System.Reflection;
using System.Text.Json;
using Athena.Core.Internal.Samples;
using Athena.Desktop.Configuration;

namespace Athena.Core.Internal.Helpers;

internal class SamplesHelper
{
    public static async Task Generate(ConfigPaths configPaths, JsonSerializerOptions serializerOptions)
    {
        // Get all classes with the ISample interface
        var sampleTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(assemblyType => typeof(ISample).IsAssignableFrom(assemblyType) && !assemblyType.IsInterface);
        
        // Instantiate all classes with the ISample interface and add them to a list
        var samples = sampleTypes.Select(type => (ISample)Activator.CreateInstance(type)!).ToList();
        
        // Select all samples and save them to disk
        var tasks = samples.Select(async sample =>
            await sample.SaveToDisk(configPaths, serializerOptions));
        await Task.WhenAll(tasks);
    }
}
