namespace Task_Management.Domain.Interfaces;

// Local-disk file storage for attachments. Files live on the machine that
// hosts the API (works fully offline on a LAN); everyone who can reach the
// API can download them through the access-checked endpoints.
public interface IFileStorageService
{
    /// <summary>Saves the content under the given relative path and returns it.</summary>
    Task<string> SaveAsync(Stream content, string relativePath, CancellationToken cancellationToken = default);

    /// <summary>Opens a stored file for reading, or null if it no longer exists.</summary>
    Stream? OpenRead(string relativePath);

    /// <summary>Deletes a stored file (no-op if missing).</summary>
    void Delete(string relativePath);
}
