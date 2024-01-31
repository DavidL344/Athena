using System.Text.Json.Serialization;

namespace Athena.Core.Model.Opener;

public class FileExtension
{
    /// <summary>
    /// A friendly name of the file extension. 
    /// </summary>
    /// <example>
    /// HTML document<br />
    /// MP3 audio<br />
    /// Zip archive
    /// </example>
    [JsonRequired]
    public required string FriendlyName { get; set; }
    
    /// <summary>
    /// A list of app entries that support the file extension.
    /// </summary>
    /// <example>
    /// firefox.open<br />
    /// firefox.open-private<br />
    /// foobar2000.play<br />
    /// foobar2000.enqueue
    /// unzip.extract<br />
    /// unzip.list<br />
    /// </example>
    [JsonRequired]
    public required string[] AppList { get; set; }

    /// <summary>
    /// If no entry is specified manually, this app entry will be used.
    /// </summary>
    /// <remarks>The default app entry must be in the <see cref="AppList" />.</remarks>
    /// <example>
    /// firefox.open<br />
    /// </example>
    public string DefaultApp { get; set; } = "";
}
