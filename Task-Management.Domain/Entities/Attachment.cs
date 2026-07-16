namespace Task_Management.Domain.Entities;

public class Attachment : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    // Relative stored path under the configured attachments folder
    // (e.g. "12/3f2a...bin") - files live on the API host's local disk so the
    // whole system stays offline/LAN-only.
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public int? UploadedById { get; set; }
    public User? UploadedBy { get; set; }
}
