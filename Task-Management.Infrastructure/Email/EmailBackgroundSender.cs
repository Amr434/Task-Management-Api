using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Task_Management.Application.Common.Interfaces;

namespace Task_Management.Infrastructure.Email;

// Sends queued emails over SMTP. Keeps one connection open while there's a
// backlog and closes it once the queue is empty. A failed email is logged and
// dropped; it never affects the request that queued it.
public class EmailBackgroundSender : BackgroundService
{
    private readonly EmailQueue _queue;
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailBackgroundSender> _logger;

    public EmailBackgroundSender(EmailQueue queue, IOptions<EmailSettings> settings, ILogger<EmailBackgroundSender> logger)
    {
        _queue = queue;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.IsConfigured)
        {
            _logger.LogWarning("EmailSettings:SenderEmail / AppPassword are not set; email notifications are off.");
            return;
        }

        using var client = new SmtpClient();
        // The certificate itself is always validated (issuer, dates, host name).
        // This only controls the extra online "has it been revoked?" lookup,
        // which fails on networks that block the certificate authorities'
        // servers ("An incomplete certificate revocation check occurred") and
        // would then stop every email. See EmailSettings.CheckCertificateRevocation.
        client.CheckCertificateRevocation = _settings.CheckCertificateRevocation;
        try
        {
            while (await _queue.Reader.WaitToReadAsync(stoppingToken))
            {
                while (_queue.Reader.TryRead(out var message))
                {
                    await SendAsync(client, message, stoppingToken);
                }

                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // App is shutting down.
        }
    }

    private async Task SendAsync(SmtpClient client, EmailMessage message, CancellationToken ct)
    {
        try
        {
            if (!client.IsConnected)
            {
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, ct);
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword, ct);
            }

            var mime = new MimeMessage();
            mime.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            mime.To.Add(MailboxAddress.Parse(message.To));
            mime.Subject = message.Subject;
            mime.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();

            await client.SendAsync(mime, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to send email \"{Subject}\" to {To}", message.Subject, message.To);
            // Start fresh for the next message in case the connection broke.
            if (client.IsConnected)
            {
                try { await client.DisconnectAsync(true, ct); } catch { }
            }
        }
    }
}
