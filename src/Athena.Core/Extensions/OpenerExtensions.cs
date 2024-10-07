using Athena.Core.Model;

namespace Athena.Core.Extensions;

public static class OpenerExtensions
{
    public static int GetAppEntryIndex(this IOpener opener, string entryName)
        => opener.AppList.IndexOf(entryName);
}
