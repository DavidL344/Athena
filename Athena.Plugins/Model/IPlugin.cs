using Athena.Plugins.Model.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Athena.Plugins.Model;

public interface IPlugin
{
    public void RegisterServices(IServiceCollection services);
    public Task<int> Run(IPluginContext context);
}
