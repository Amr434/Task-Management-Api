using Task_Management.Application.Features.Users.DTOs;

namespace Task_Management.Application.Features.Attachments.DTOs;

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public DateTime UploadedAt { get; set; }
    public int TaskItemId { get; set; }
    public UserDto? UploadedBy { get; set; }
}

// Streamed file returned by the download query; the controller turns it into
// a FileResult.
public class AttachmentFileDto
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
}
