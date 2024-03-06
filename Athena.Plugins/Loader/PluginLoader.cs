using System.Reflection;
using Athena.Plugins.Internal;
using Athena.Plugins.Model;
using Microsoft.Extensions.Logging;

namespace Athena.Plugins.Loader;

public class PluginLoader
{
    private readonly ILogger<PluginLoader> _logger;

    public PluginLoader(ILogger<PluginLoader> logger)
    {
        _logger = logger;
    }

    public IEnumerable<IPlugin> LoadAll(string pluginDir)
    {
        _logger.LogInformation("Loading plugins from {PluginDirectory}", pluginDir);
        
        var pluginPaths = Directory.GetFiles(pluginDir, "*.dll", SearchOption.AllDirectories);
        
        return pluginPaths.SelectMany(pluginPath =>
        {
            try
            {
                var pluginAssembly = LoadPlugin(pluginPath);
                return LoadAssemblyTypes(pluginAssembly);
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to load plugin {PluginPath}: {ErrorMessage}",
                    Path.GetFileName(pluginPath), e.Message);
            }
            return Enumerable.Empty<IPlugin>();
        });
    }
    
    private Assembly LoadPlugin(string pluginPath)
    {
        
        _logger.LogInformation("Loading commands from: {PluginPath}", pluginPath);
        
        var loadContext = new LoadContext(Path.GetFullPath(pluginPath));
        return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));
    }
    
    private IEnumerable<IPlugin> LoadAssemblyTypes(Assembly assembly)
    {
        var count = 0;

        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type)) continue;
            if (Activator.CreateInstance(type) is not IPlugin result) continue;
            count++;
            yield return result;
        }

        if (count != 0) yield break;
        var availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
        _logger.LogError("Plugin {PluginAssembly} from {PluginPath} doesn't have any valid types!\n" +
                         "Available types: {AvailableTypes}", assembly, assembly.Location, availableTypes);
    }
}
