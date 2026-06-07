namespace Analytics.Service.Models;

public class EventStat
{
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int TotalRegistrations { get; set; }
    public decimal TotalRevenue { get; set; }
    public int UniqueAttendees { get; set; }
}
