using Athena.Core.Model;

namespace Athena.Core.Internal.Samples;

internal interface ISample
{
    public Dictionary<string, AppEntry> Entries { get; }
    public Dictionary<string, FileExtension> FileExtensions { get; }
    public Dictionary<string, Protocol> Protocols { get; }
    public List<string> MimeTypes { get; }
}
