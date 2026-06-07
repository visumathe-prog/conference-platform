namespace Analytics.Service.Models;

public class RegistrationEvent
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
