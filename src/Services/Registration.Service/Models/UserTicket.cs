namespace Registration.Service.Models;

public class UserTicket
{
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string QrCode { get; set; } = string.Empty;
}
