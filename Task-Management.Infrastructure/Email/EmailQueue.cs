using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Task_Management.Application.Common.Interfaces;

namespace Task_Management.Infrastructure.Email;

// In-memory queue drained by EmailBackgroundSender. Emails still queued when
// the app stops are lost — acceptable for notifications, which also show in-app.
public class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>();
    private readonly bool _enabled;

    public EmailQueue(IOptions<EmailSettings> settings)
    {
        _enabled = settings.Value.IsConfigured;
    }

    public ChannelReader<EmailMessage> Reader => _channel.Reader;

    public void Enqueue(EmailMessage message)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(message.To)) return;
        _channel.Writer.TryWrite(message);
    }
}
