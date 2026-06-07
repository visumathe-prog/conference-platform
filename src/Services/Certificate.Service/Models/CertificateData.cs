namespace Certificate.Service.Models;

public class CertificateData
{
    public string CertificateId { get; set; } = Guid.NewGuid().ToString();
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public Guid UserId { get; set; }
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
}
