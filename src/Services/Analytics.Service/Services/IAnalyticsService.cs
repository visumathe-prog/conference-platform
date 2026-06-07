using Analytics.Service.Models;

namespace Analytics.Service.Services;

public interface IAnalyticsService
{
    Task<EventStat> GetEventStatisticsAsync(Guid eventId);
    Task<List<EventStat>> GetTopEventsAsync(int limit);
    Task<Dictionary<DateTime, int>> GetDailyRegistrationsAsync(int days);
    Task RecordRegistrationEventAsync(RegistrationEvent registrationEvent);
}
