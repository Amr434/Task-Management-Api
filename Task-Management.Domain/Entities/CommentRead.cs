namespace Task_Management.Domain.Entities;

// "User X has read comment Y": drives the unread count on Replies. Stored in
// the database so reading a reply on the web also clears it on mobile.
public class CommentRead : BaseEntity
{
    public int CommentId { get; set; }
    public Comment? Comment { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}
