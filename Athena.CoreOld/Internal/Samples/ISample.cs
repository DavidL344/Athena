using Athena.CoreOld.Model.Entry;
using Athena.CoreOld.Model.Opener;

namespace Athena.CoreOld.Internal.Samples;

public interface ISample
{
    public Dictionary<string, AppEntry> Entries { get; }
    public Dictionary<string, FileExtension> FileExtensions { get; }
    public Dictionary<string, Protocol> Protocols { get; }
}
