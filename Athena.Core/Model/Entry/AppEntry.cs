using System.Text.Json.Serialization;

namespace Athena.Core.Model.Entry;

public class AppEntry
{
    /// <summary>
    /// The displayed name of the app entry.
    /// </summary>
    /// <example>
    /// foobar2000 (play)<br />
    /// foobar2000 (add to queue)<br />
    /// Firefox (new tab)<br />
    /// unzip (list files)
    /// </example>
    [JsonRequired]
    public required string Name { get; set; }

    /// <summary>
    /// An app entry type.
    /// Influences whether the entry is shown in the file, protocol, or both app pickers.
    /// </summary>
    /// <value>all/file/protocol</value>
    public EntryType Type { get; set; } = EntryType.All;

    /// <summary>
    /// The path to the executable with the arguments.
    /// The shorter versions of apps registered to $PATH IS preferred for cross-platform reasons.
    /// </summary>
    /// <example>
    /// /snap/bin/foobar2000<br />
    /// /usr/bin/firefox<br />
    /// unzip
    /// </example>
    [JsonRequired]
    public required string Path { get; set; }

    /// <summary>
    /// The opened file's path with any additional arguments.
    /// </summary>
    /// <example>
    /// $FILE<br />
    /// /add $FILE<br />
    /// --new-tab $URL<br />
    /// -l $FILE
    /// </example>
    [JsonRequired]
    public required string Arguments { get; set; }
}
