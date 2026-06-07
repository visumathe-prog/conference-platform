using Confluent.Kafka;
using System.Text.Json;
using Registration.Service.Models;
using Registration.Service.Events;

namespace Registration.Service.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private static readonly Dictionary<Guid, int> _eventSeats = new();

    public RegistrationService(IProducer<string, string> producer, IConfiguration configuration)
    {
        _producer = producer;
        _topic = configuration["Kafka:Topic"] ?? "registration-events";
    }

    public async Task<bool> RegisterAsync(Guid eventId, Guid userId, string ticketType)
    {
        lock (_eventSeats)
        {
            if (!_eventSeats.ContainsKey(eventId))
                _eventSeats[eventId] = 200; // Default seats

            if (_eventSeats[eventId] <= 0)
                return false;

            _eventSeats[eventId]--;
        }

        var registrationEvent = new RegistrationCompletedEvent
        {
            EventId = eventId,
            UserId = userId,
            TicketType = ticketType,
            Amount = ticketType == "vip" ? 1000 : 500,
            CreatedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(registrationEvent);
        await _producer.ProduceAsync(_topic, new Message<string, string> { Key = eventId.ToString(), Value = json });

        return true;
    }

    public Task<List<UserTicket>> GetUserRegistrationsAsync(Guid userId)
    {
        var tickets = new List<UserTicket>
        {
            new() { EventId = Guid.NewGuid(), EventTitle = "Tech Conference 2026", RegistrationDate = DateTime.UtcNow, QrCode = "data:image/png;base64,..." }
        };
        return Task.FromResult(tickets);
    }
}
