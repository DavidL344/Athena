using System.Reflection;
using Athena.Core.Parser.Shared;
using Athena.Plugins.Loader;
using Athena.Plugins.Model;
using Athena.Plugins.Model.Context;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Athena.Plugins.DependencyInjection;

public static class PluginExtensions
{
    public static void AddPlugins(this IServiceCollection services)
    {
        var configPaths = services.BuildServiceProvider().GetRequiredService<Dictionary<ConfigType, string>>();
        services.AddPlugins(configPaths[ConfigType.Plugins]);
    }
    
    public static void AddPlugins(this IServiceCollection services, string pluginDirectory)
    {
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<PluginLoader>>();
        var pluginLoader = new PluginLoader(logger);
        pluginLoader.LoadAll(pluginDirectory).ToList().ForEach(plugin =>
        {
            plugin.RegisterServices(services);
        });
    }
    
    public static Dictionary<string, int> RunPlugins(this CoconaApp app)
    {
        var pluginTypes = GetPluginTypes(typeof(IPlugin));
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var results = new Dictionary<string, int>();
        
        foreach (var pluginType in pluginTypes)
        {
            var context = new PluginContext
            {
                Logger = (ILogger<IPlugin>)loggerFactory.CreateLogger(pluginType.AsType()),
                Result = 0
            };
            
            pluginType.GetMethod(nameof(IPlugin.Run))!
                .Invoke(null, [context]);
            
            results.Add(pluginType.Name, context.Result);
        }

        return results;
    }
    
    private static IEnumerable<TypeInfo> GetPluginTypes(Type typeMarker)
    {
        var endpointTypes = typeMarker.Assembly.DefinedTypes
            .Where(x => x is { IsAbstract: false, IsInterface: false } && 
                        typeMarker.IsAssignableFrom(x));
        return endpointTypes;
    }
}
