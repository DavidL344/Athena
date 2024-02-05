using Athena.Core.Model.Entry;
using Athena.Core.Model.Opener;

namespace Athena.Core.Internal.Samples;

public interface ISample
{
    public Dictionary<string, AppEntry> Entries { get; }
    public Dictionary<string, FileExtension> FileExtensions { get; }
    public Dictionary<string, Protocol> Protocols { get; }
}
