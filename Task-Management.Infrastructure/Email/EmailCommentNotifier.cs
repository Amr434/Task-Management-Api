using Microsoft.Extensions.Options;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Domain.Interfaces;

namespace Task_Management.Infrastructure.Email;

// A new comment pops up live for everyone on the project, but only the person
// it's assigned to (an action item) gets an email.
public class EmailCommentNotifier : ICommentNotifier
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailQueue _queue;
    private readonly EmailSettings _settings;

    public EmailCommentNotifier(IUnitOfWork unitOfWork, IEmailQueue queue, IOptions<EmailSettings> settings)
    {
        _unitOfWork = unitOfWork;
        _queue = queue;
        _settings = settings.Value;
    }

    public async Task CommentAddedAsync(IEnumerable<int> recipientUserIds, CommentDto comment)
    {
        // Recipients never include the author, so self-assigned comments send nothing.
        if (comment.AssignedTo is null || !recipientUserIds.Contains(comment.AssignedTo.Id)) return;

        var assignee = (await EmailRecipients.LoadAsync(_unitOfWork, new[] { comment.AssignedTo.Id })).FirstOrDefault();
        if (assignee is null) return;

        var author = comment.Author is null
            ? "Someone"
            : $"{comment.Author.FirstName} {comment.Author.LastName}".Trim();
        var task = string.IsNullOrWhiteSpace(comment.TaskTitle) ? "a task" : $"\"{comment.TaskTitle}\"";

        var subject = $"{author} assigned you a comment on {task}";
        var link = $"{_settings.AppBaseUrl.TrimEnd('/')}/assigned-comments";
        var html = EmailTemplates.Notification(subject, $"{author} wrote on {task}:", comment.Text, "View comment", link);

        _queue.Enqueue(new EmailMessage(assignee.Email, subject, html));
    }
}
