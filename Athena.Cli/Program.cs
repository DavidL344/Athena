using Athena.Cli.Commands.Internal;
using Athena.Core.DependencyInjection;
using Athena.Plugins.DependencyInjection;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = CoconaApp.CreateBuilder(args, x =>
{
    if (!OperatingSystem.IsWindows())
        x.EnableShellCompletionSupport = true;
    x.TreatPublicMethodsAsCommands = false;
});

builder.Services.AddLogging(x =>
{
    x.AddConsole();
});

builder.Services.AddAthenaCore();
builder.Services.AddPlugins();

var app = builder.Build();

app.RegisterCommands();
app.RunPlugins();

app.Run();
