using Task_Management.Domain.Entities;

namespace Task_Management.Application.Features.Users;

public static class AvatarUrls
{
    // Allowed picture types and the size limit for profile pictures.
    public const long MaxBytes = 5 * 1024 * 1024;

    public static readonly IReadOnlyDictionary<string, string> ExtensionsByContentType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/jpg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
            ["image/gif"] = ".gif",
            ["image/heic"] = ".heic",
        };

    // URL the apps put in an <img>/<Image>, relative to the API base URL.
    // The ?v= part changes with every upload so old pictures aren't cached.
    public static string? For(User user) =>
        string.IsNullOrEmpty(user.AvatarPath)
            ? null
            : $"Users/{user.Id}/avatar?v={Path.GetFileNameWithoutExtension(user.AvatarPath)}";

    public static string ContentTypeFor(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        ".heic" => "image/heic",
        _ => "image/jpeg",
    };
}
