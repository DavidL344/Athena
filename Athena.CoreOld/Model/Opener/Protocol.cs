using System.Text.Json.Serialization;

namespace Athena.CoreOld.Model.Opener;

public class Protocol : IOpener
{
    /// <summary>
    /// The name of the protocol before the :// part.
    /// </summary>
    /// <remarks>The schema gets populated to x-scheme-handler/<see cref="Name" /> automatically.</remarks>
    /// <example>
    /// http<br />
    /// https<br />
    /// mailto<br />
    /// ssh
    /// </example>
    [JsonRequired]
    [JsonPropertyName("schemaName")]
    public required string Name { get; set; }
    
    /// <summary>
    /// A list of app entries that support the protocol.
    /// </summary>
    /// <example>
    /// firefox.open<br />
    /// firefox.open-private<br />
    /// foobar2000.play<br />
    /// foobar2000.enqueue
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