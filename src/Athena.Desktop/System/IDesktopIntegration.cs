namespace Athena.Desktop.System;

public interface IDesktopIntegration
{
    /// <summary>
    /// Add the Athena entry to the system.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: adds the entry into the Windows Registry without associating it<br />
    /// <b>Linux</b>: adds a .desktop file to ~/.local/share/applications and symlinks the executable to ~/.local/bin
    /// </remarks>
    void RegisterEntry();
    
    /// <summary>
    /// Remove the Athena entry from the system.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: removes the entry from the Windows Registry<br />
    /// <b>Linux</b>: removes the .desktop file from ~/.local/share/applications and the symlink from ~/.local/bin
    /// </remarks>
    void DeregisterEntry();
    
    /// <summary>
    /// Add Athena to the list of apps that can open files.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: associates the Athena entry to be openable with the file extensions in the Windows Registry<br />
    /// <b>Linux</b>: adds the MIME types to the Athena XDG Desktop file
    /// </remarks>
    /// <param name="fileExtensionsOrMimeTypes">The file extensions (Windows) or a MIME types (Linux) to associate</param>
    void AssociateWithApps(IEnumerable<string> fileExtensionsOrMimeTypes);
    
    /// <summary>
    /// Add Athena to the list of apps that can open files.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: associates the Athena entry to be openable with the file extension in the Windows Registry<br />
    /// <b>Linux</b>: adds the MIME type to the Athena XDG Desktop file
    /// </remarks>
    /// <param name="fileExtensionOrMimeType">The file extension (Windows) or a MIME type (Linux) to associate</param>
    /// <param name="source">Whether the shell should be updated immediately after adding the association</param>
    void AssociateWithApp(string fileExtensionOrMimeType, bool source = true);
    
    /// <summary>
    /// Remove Athena from the list of apps that can open files.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: removes the Athena entry from being openable with the file extensions in the Windows Registry<br />
    /// <b>Linux</b>: removes the MIME types from the Athena XDG Desktop file
    /// </remarks>
    /// <param name="fileExtensionsOrMimeTypes">The file extension (Windows) or a MIME type (Linux) to remove the association from</param>
    void DissociateFromApps(IEnumerable<string> fileExtensionsOrMimeTypes);
    
    /// <summary>
    /// Remove Athena from the list of apps that can open files.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: removes the Athena entry from being openable with the file extension in the Windows Registry<br />
    /// <b>Linux</b>: removes the MIME type from the Athena XDG Desktop file
    /// </remarks>
    /// <param name="fileExtensionOrMimeType">The file extension (Windows) or a MIME type (Linux) to remove the association from</param>
    /// <param name="source">Whether the shell should be updated immediately after removing the association</param>
    void DissociateFromApp(string fileExtensionOrMimeType, bool source = true);
    
    /// <summary>
    /// Check if Athena is registered with the system.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: returns true when the Athena entry is in the Windows Registry<br />
    /// <b>Linux</b>: returns true when the Athena symlink and XDG Desktop file are present
    /// </remarks>
    bool IsRegistered { get; }
    
    /// <summary>
    /// Output the current status of Athena's integration with the system to the console.
    /// </summary>
    /// <remarks>
    /// Windows: shows the PATH registration status
    /// Linux: shows the symlink and xdg-desktop file registration status
    /// </remarks>
    string ConsoleStatus();
}
