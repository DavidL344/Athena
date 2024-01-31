using Athena.Core.Model.Entry;
using Athena.Core.Model.Opener;

namespace Athena.Core.Internal;

public static class DefinitionExtensions
{
    public static bool HasEntries(this FileExtension fileExtensionDefinition)
        => fileExtensionDefinition.AppList.Length > 0;
    
    public static bool HasEntry(this FileExtension fileExtensionDefinition, int entryIndex)
        => entryIndex >= 0 && entryIndex < fileExtensionDefinition.AppList.Length;
    
    public static AppEntry ExpandEnvironmentVariables(this AppEntry appEntryDefinition, string filePath)
    {
        appEntryDefinition.Path = Environment.ExpandEnvironmentVariables(appEntryDefinition.Path
            .Replace("~", "%HOME%"));

        appEntryDefinition.Arguments = Environment.ExpandEnvironmentVariables(appEntryDefinition.Arguments
            .Replace("~", "%HOME%")
            .Replace("$FILE", filePath)
            .Replace("$URL", filePath));
        
        return appEntryDefinition;
    }
}
