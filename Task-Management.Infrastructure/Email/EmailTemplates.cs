using System.Net;
using System.Text;

namespace Task_Management.Infrastructure.Email;

// A line in a list email: its text and where it links.
internal record EmailListItem(string Text, string Link);

// A titled group of lines, e.g. "Overdue" in the summary email.
internal record EmailListSection(string Title, IReadOnlyList<EmailListItem> Items);

// One shared layout for every email. All values passed in here are
// user-supplied text, so they're HTML-encoded.
internal static class EmailTemplates
{
    // Footer of notification emails: where to turn them off.
    public const string SettingsHint =
        "You're getting this because of activity in Task Management. You can choose which emails you get under your profile → Notification settings.";

    // Footer of account emails (welcome, password reset), which always go out.
    public const string AccountFooter = "This email was sent by Task Management about your account.";

    public static string Notification(string heading, string message, string? detail, string buttonText, string link,
        string footer = SettingsHint)
    {
        var detailHtml = string.IsNullOrWhiteSpace(detail)
            ? string.Empty
            : $"""<p style="margin:0 0 20px;padding:12px 14px;background:#f4f5f7;border-radius:6px;color:#333;font-size:14px;">{Encode(detail).Replace("\n", "<br>")}</p>""";

        var body = $"""
            <h2 style="margin:0 0 12px;font-size:18px;color:#1a1d24;">{Encode(heading)}</h2>
            <p style="margin:0 0 16px;font-size:15px;line-height:1.5;color:#333;">{Encode(message)}</p>
            {detailHtml}
            {Button(buttonText, link)}
            """;
        return Layout(body, footer);
    }

    // The daily digest and the summary: a heading, an intro line, then
    // sections of linked lines.
    public static string List(string heading, string intro, IEnumerable<EmailListSection> sections, string buttonText, string link)
    {
        var html = new StringBuilder();
        html.Append($"""<h2 style="margin:0 0 12px;font-size:18px;color:#1a1d24;">{Encode(heading)}</h2>""");
        html.Append($"""<p style="margin:0 0 16px;font-size:15px;line-height:1.5;color:#333;">{Encode(intro)}</p>""");

        foreach (var section in sections.Where(s => s.Items.Count > 0))
        {
            html.Append($"""<h3 style="margin:18px 0 8px;font-size:14px;color:#1a1d24;">{Encode(section.Title)}</h3>""");
            html.Append("""<ul style="margin:0 0 8px;padding-left:20px;font-size:14px;line-height:1.6;color:#333;">""");
            foreach (var item in section.Items)
            {
                html.Append($"""<li><a href="{Encode(item.Link)}" style="color:#4f46e5;text-decoration:none;">{Encode(item.Text)}</a></li>""");
            }
            html.Append("</ul>");
        }

        html.Append("""<div style="height:12px;"></div>""");
        html.Append(Button(buttonText, link));
        return Layout(html.ToString(), SettingsHint);
    }

    private static string Button(string text, string link) =>
        $"""<a href="{Encode(link)}" style="display:inline-block;padding:10px 18px;background:#4f46e5;color:#ffffff;text-decoration:none;border-radius:6px;font-size:14px;">{Encode(text)}</a>""";

    private static string Layout(string body, string footer) => $"""
        <!doctype html>
        <html>
        <body style="margin:0;padding:24px;background:#f6f7fb;font-family:Segoe UI,Arial,sans-serif;">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0">
            <tr><td align="center">
              <table role="presentation" width="520" cellspacing="0" cellpadding="0" style="max-width:520px;background:#ffffff;border-radius:10px;border:1px solid #e3e5ea;">
                <tr><td style="padding:28px 28px 8px;">
                  {body}
                </td></tr>
                <tr><td style="padding:20px 28px 24px;font-size:12px;color:#8a8f99;">
                  {Encode(footer)}
                </td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
