using Athena.Plugins.Model;
using Athena.Plugins.Model.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Athena.Plugins.Template;

public class SamplePlugin : IPlugin
{
    public void RegisterServices(IServiceCollection services)
    {
        
    }

    public Task<int> Run(IPluginContext context)
    {
        context.Logger.LogInformation("Hello, World!");
        return Task.FromResult(0);
    }
}
