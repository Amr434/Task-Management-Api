namespace Task_Management.Infrastructure.Email;

// "EmailSettings" config section. SenderEmail and AppPassword are secrets: keep
// them in user-secrets locally (dotnet user-secrets set "EmailSettings:AppPassword" ...)
// and in environment variables on the server (EmailSettings__AppPassword),
// never in appsettings.json. Without them, no emails are sent.
public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "Task Management";
    public string AppPassword { get; set; } = string.Empty;

    // Web app address, for the "Open" links in emails.
    public string AppBaseUrl { get; set; } = "http://localhost:3000";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SenderEmail) && !string.IsNullOrWhiteSpace(AppPassword);
}
