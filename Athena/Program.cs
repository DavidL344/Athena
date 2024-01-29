using Cocona;

var builder = CoconaApp.CreateBuilder(args, x =>
{
    x.EnableShellCompletionSupport = true;
    x.TreatPublicMethodsAsCommands = false;
});

var app = builder.Build();

app.Run();
