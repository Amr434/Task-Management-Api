using Task_Management.Application.Features.Users.DTOs;

namespace Task_Management.Application.Features.Comments.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public UserDto? Author { get; set; }
    public UserDto? AssignedTo { get; set; }
    public UserDto? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Task context, populated when the task is loaded with the comment
    // (always for the "Assigned Comments" view).
    public int TaskItemId { get; set; }
    public string? TaskTitle { get; set; }
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? SpaceName { get; set; }
}

public class CreateCommentDto
{
    public string Text { get; set; } = string.Empty;

    // Optional: makes it an "assigned comment" (action item) for a
    // project participant — the author themselves or anyone else.
    public int? AssignedToId { get; set; }
}
