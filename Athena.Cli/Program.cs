using Athena.Cli.Commands.Internal;
using Athena.Core.Extensions.DependencyInjection;
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

var app = builder.Build();

app.RegisterCommands();

app.Run();
