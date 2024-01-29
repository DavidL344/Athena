using Athena.Commands;
using Athena.Internal;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = CoconaApp.CreateBuilder(args, x =>
{
    x.EnableShellCompletionSupport = true;
    x.TreatPublicMethodsAsCommands = false;
});

builder.Logging.AddDebug();
builder.Services.AddLogging(x =>
{
    x.AddConsole();
});

builder.Services.AddSingleton(Vars.JsonSerializerOptions);

var app = builder.Build();

await StartupChecks.RunAsync();

app.AddCommands<RunCommands>();

app.Run();
