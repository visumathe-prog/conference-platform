namespace Registration.Service.Events;

public class RegistrationCompletedEvent
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string TicketType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
