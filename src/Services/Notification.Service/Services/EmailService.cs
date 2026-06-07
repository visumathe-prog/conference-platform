using SendGrid;
using SendGrid.Helpers.Mail;

namespace Notification.Service.Services;

public class EmailService : INotificationService
{
    private readonly SendGridClient _client;
    private readonly string _fromEmail;

    public EmailService(IConfiguration configuration)
    {
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? configuration["SendGrid:ApiKey"];
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@conference.com";
        _client = new SendGridClient(apiKey);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var msg = MailHelper.CreateSingleEmail(
            new EmailAddress(_fromEmail, "Conference Platform"),
            new EmailAddress(to),
            subject,
            body,
            body
        );

        await _client.SendEmailAsync(msg);
    }
}
