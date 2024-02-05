using Athena.Commands;
using Athena.Core.DependencyInjection;
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

builder.Environment.InitAthena();
builder.Services.AddAthenaCore();

var app = builder.Build();

app.AddCommands<RunCommands>();

app.Run();
