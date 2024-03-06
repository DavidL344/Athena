using Microsoft.Extensions.Logging;

namespace Athena.Plugins.Model.Context;

public class PluginContext : IPluginContext
{
    public ILogger<IPlugin> Logger { get; init; } = default!;
    public int Result { get; set; }
}
