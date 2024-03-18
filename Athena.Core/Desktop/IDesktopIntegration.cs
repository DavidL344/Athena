namespace Athena.Core.Desktop;

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
    /// <b>Windows</b>: associates the Athena entry to be openable with the file extension(s) in the Windows Registry<br />
    /// <b>Linux</b>: adds the Athena entry to the MIME type(s) in the ~/.local/share/applications/mimeapps.list file under [Added Associations]
    /// </remarks>
    void AssociateWithApps();
    
    /// <inheritdoc cref="AssociateWithApps"/>
    /// <param name="fileExtensionOrMimeType">The file extension (Windows) or a MIME type (Linux) to associate</param>
    void AssociateWithApp(string fileExtensionOrMimeType);
    
    /// <summary>
    /// Remove Athena from the list of apps that can open files.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: removes the Athena entry from being openable with the file extension(s) in the Windows Registry<br />
    /// <b>Linux</b>: removes the Athena entry from the MIME type(s) in the ~/.local/share/applications/mimeapps.list file under [Added Associations]
    /// </remarks>
    void DissociateFromApps();
    
    /// <inheritdoc cref="DissociateFromApps"/>
    /// <param name="fileExtensionOrMimeType">The file extension (Windows) or a MIME type (Linux) to remove the association from</param>
    void DissociateFromApp(string fileExtensionOrMimeType);
    
    /// <summary>
    /// Set Athena as the default app for opening files.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: sets the Athena entry as the default app for the file extension(s) in the Windows Registry<br />
    /// <b>Linux</b>: sets the Athena entry as the default app for the MIME type(s) in the ~/.local/share/applications/mimeapps.list file under [Default Applications]
    /// </remarks>
    void SetAsDefault();
    
    /// <inheritdoc cref="SetAsDefault"/>
    /// <param name="fileExtensionOrMimeType">The file extension (Windows) or a MIME type (Linux) to set Athena as the default app for</param>
    void SetAsDefaultFor(string fileExtensionOrMimeType);
    
    /// <summary>
    /// Remove Athena from being the default app for opening files.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: removes the Athena entry from being the default app for the file extension(s) in the Windows Registry<br />
    /// <b>Linux</b>: removes the Athena entry from being the default app for the MIME type(s) in the ~/.local/share/applications/mimeapps.list file under [Default Applications]
    /// </remarks>
    void UnsetDefault();
    
    /// <inheritdoc cref="UnsetDefault"/>
    /// <param name="fileExtensionOrMimeType">The file extension (Windows) or a MIME type (Linux) to remove Athena as the default app for</param>
    void UnsetDefaultFrom(string fileExtensionOrMimeType);
    
    /// <summary>
    /// Backup all entries before making any changes to the system.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: saves the entries to a *.reg file<br />
    /// <b>Linux</b>: copies the entries to a *.list.bak file
    /// </remarks>
    /// <param name="backupPath">The path to save the backup to</param>
    void BackupAllEntries(string backupPath);
    
    /// <summary>
    /// Restore all entries from a backup.
    /// </summary>
    /// <remarks>
    /// <b>Windows</b>: restores the entries from a *.reg file<br />
    /// <b>Linux</b>: copies the entries from a *.list.bak file
    /// </remarks>
    /// <param name="backupPath">The path to the backup file</param>
    void RestoreAllEntries(string backupPath);
    
    /// <summary>
    /// Output the current status of Athena's integration with the system to the console.
    /// </summary>
    /// <remarks>
    /// Windows: shows the PATH registration status
    /// Linux: shows the symlink and xdg-desktop file registration status
    /// </remarks>
    string ConsoleStatus();
}
