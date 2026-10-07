using Microsoft.Extensions.Options;
using Task_Management.Application.Common.Interfaces;

namespace Task_Management.Infrastructure.Email;

public class EmailAccountNotifier : IAccountNotifier
{
    private readonly IEmailQueue _queue;
    private readonly EmailSettings _settings;

    public EmailAccountNotifier(IEmailQueue queue, IOptions<EmailSettings> settings)
    {
        _queue = queue;
        _settings = settings.Value;
    }

    public Task AccountCreatedAsync(string email, string firstName, string createdByName, string temporaryPassword)
    {
        var greeting = string.IsNullOrWhiteSpace(firstName) ? "Hi," : $"Hi {firstName},";
        var subject = "Your Task Management account is ready";
        var message = $"{greeting} {createdByName} added you to Task Management. "
            + "Sign in with the details below — you'll be asked to choose your own password the first time.";
        var detail = $"Email: {email}\nTemporary password: {temporaryPassword}";
        var link = $"{_settings.AppBaseUrl.TrimEnd('/')}/login";

        var html = EmailTemplates.Notification(subject, message, detail, "Sign in", link);
        _queue.Enqueue(new EmailMessage(email, subject, html));
        return Task.CompletedTask;
    }

    public Task PasswordResetRequestedAsync(string email, string firstName, string resetToken, int validForMinutes)
    {
        var greeting = string.IsNullOrWhiteSpace(firstName) ? "Hi," : $"Hi {firstName},";
        var subject = "Reset your Task Management password";
        var message = $"{greeting} we received a request to reset your password. "
            + "Use the button below to choose a new one.";
        var detail = $"This link works once and expires in {validForMinutes} minutes.\n"
            + "If you didn't ask for this, you can ignore this email. Your password stays the same.";
        // The token is hex, so it needs no URL encoding.
        var link = $"{_settings.AppBaseUrl.TrimEnd('/')}/reset-password?token={resetToken}";

        var html = EmailTemplates.Notification(subject, message, detail, "Choose a new password", link);
        _queue.Enqueue(new EmailMessage(email, subject, html));
        return Task.CompletedTask;
    }
}
