using Microsoft.Extensions.Configuration;
using Task_Management.Domain.Interfaces;

namespace Task_Management.Infrastructure.Services;

// Stores attachment files on the API host's local disk, OUTSIDE the
// application folder: user data must never live in the deploy tree, where a
// redeploy wipes it and it bloats the project directory. The base folder
// comes from configuration ("Attachments:StoragePath"); the default is the
// OS application-data location (C:\ProgramData\TaskManagement\attachments on
// Windows, /var/lib equivalent on Linux). Shared hosts without a writable
// ProgramData (e.g. MonsterASP) must set the config value explicitly.
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["Attachments:StoragePath"]
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "TaskManagement", "attachments");
    }

    // Resolve and verify the path stays inside the base folder (defends
    // against path traversal via a tampered stored name).
    private string Resolve(string relativePath)
    {
        var full = Path.GetFullPath(Path.Combine(_basePath, relativePath));
        var baseFull = Path.GetFullPath(_basePath);
        if (!full.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid attachment path.");
        }
        return full;
    }

    public async Task<string> SaveAsync(Stream content, string relativePath, CancellationToken cancellationToken = default)
    {
        var full = Resolve(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await using var target = File.Create(full);
        await content.CopyToAsync(target, cancellationToken);
        return relativePath;
    }

    public Stream? OpenRead(string relativePath)
    {
        var full = Resolve(relativePath);
        return File.Exists(full) ? File.OpenRead(full) : null;
    }

    public void Delete(string relativePath)
    {
        var full = Resolve(relativePath);
        if (File.Exists(full)) File.Delete(full);
    }
}
