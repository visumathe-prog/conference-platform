using Confluent.Kafka;
using System.Text.Json;

namespace Identity.Service.Infrastructure.Services;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default);
}

public class KafkaEventBus : IEventBus
{
    private readonly IProducer<string, string> _producer;

    public KafkaEventBus(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(@event);
        var message = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = json
        };

        await _producer.ProduceAsync(typeof(T).Name, message, ct);
    }
}
