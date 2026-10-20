namespace Task_Management.Application.Common.Interfaces;

public record EmailMessage(string To, string Subject, string HtmlBody);

// Implemented in Infrastructure: emails are queued and sent in the background,
// so a slow or failing mail server never slows down or fails a request.
// Drops messages when email isn't configured.
public interface IEmailQueue
{
    void Enqueue(EmailMessage message);
}
