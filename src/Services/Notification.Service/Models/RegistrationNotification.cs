namespace Notification.Service.Models;

public class RegistrationNotification
{
    public string Email { get; set; } = string.Empty;
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string CertificateId { get; set; } = string.Empty;
}
