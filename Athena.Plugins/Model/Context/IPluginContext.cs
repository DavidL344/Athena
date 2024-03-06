using Microsoft.Extensions.Logging;

namespace Athena.Plugins.Model.Context;

public interface IPluginContext
{
    ILogger<IPlugin> Logger { get; init; }
}
