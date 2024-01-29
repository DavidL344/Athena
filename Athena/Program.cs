using Athena.Commands;
using Athena.Internal;
using Cocona;
using Microsoft.Extensions.DependencyInjection;

var builder = CoconaApp.CreateBuilder(args, x =>
{
    x.EnableShellCompletionSupport = true;
    x.TreatPublicMethodsAsCommands = false;
});

builder.Services.AddSingleton(Vars.JsonSerializerOptions);

var app = builder.Build();

await StartupChecks.RunAsync();

app.AddCommands<RunCommands>();

app.Run();
