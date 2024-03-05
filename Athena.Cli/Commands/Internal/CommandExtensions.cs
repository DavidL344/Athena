using System.Reflection;
using Cocona;
using Cocona.Builder;

namespace Athena.Cli.Commands.Internal;

public static class CommandExtensions
{
    public static void RegisterCommands(this CoconaApp app)
    {
        app.RegisterCommands(typeof(ICommands));
    }
    
    public static void RegisterCommands<TMarker>(this CoconaApp app)
    {
        app.RegisterCommands(typeof(TMarker));
    }
    
    public static void RegisterCommands(this ICoconaCommandsBuilder app, Type typeMarker)
    {
        var commandTypes = GetCommandTypes(typeMarker);
        
        app.AddCommands(commandTypes);
    }
    
    private static IEnumerable<TypeInfo> GetCommandTypes(Type typeMarker)
    {
        var commandTypes = typeMarker.Assembly.DefinedTypes
            .Where(x => x is { IsAbstract: false, IsInterface: false });
        
        return commandTypes;
    }
}
