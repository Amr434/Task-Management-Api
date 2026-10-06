using System.Net;

namespace Task_Management.Infrastructure.Email;

// One shared layout for every notification email. All values passed in here
// are user-supplied text, so they're HTML-encoded.
internal static class EmailTemplates
{
    public static string Notification(string heading, string message, string? detail, string buttonText, string link)
    {
        var detailHtml = string.IsNullOrWhiteSpace(detail)
            ? string.Empty
            : $"""<p style="margin:0 0 20px;padding:12px 14px;background:#f4f5f7;border-radius:6px;color:#333;font-size:14px;">{Encode(detail).Replace("\n", "<br>")}</p>""";

        return $"""
            <!doctype html>
            <html>
            <body style="margin:0;padding:24px;background:#f6f7fb;font-family:Segoe UI,Arial,sans-serif;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0">
                <tr><td align="center">
                  <table role="presentation" width="520" cellspacing="0" cellpadding="0" style="max-width:520px;background:#ffffff;border-radius:10px;border:1px solid #e3e5ea;">
                    <tr><td style="padding:28px 28px 8px;">
                      <h2 style="margin:0 0 12px;font-size:18px;color:#1a1d24;">{Encode(heading)}</h2>
                      <p style="margin:0 0 16px;font-size:15px;line-height:1.5;color:#333;">{Encode(message)}</p>
                      {detailHtml}
                      <a href="{Encode(link)}" style="display:inline-block;padding:10px 18px;background:#4f46e5;color:#ffffff;text-decoration:none;border-radius:6px;font-size:14px;">{Encode(buttonText)}</a>
                    </td></tr>
                    <tr><td style="padding:20px 28px 24px;font-size:12px;color:#8a8f99;">
                      You're getting this because of activity in Task Management.
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
