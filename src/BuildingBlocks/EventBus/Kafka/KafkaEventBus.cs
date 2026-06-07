using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace BuildingBlocks.EventBus.Kafka;

public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : class;
}

public class KafkaEventBus : IEventBus
{
    private readonly IProducer<string, string> _producer;

    public KafkaEventBus(string bootstrapServers)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(T @event) where T : class
    {
        var json = JsonSerializer.Serialize(@event);
        await _producer.ProduceAsync(typeof(T).Name, new Message<string, string> { Value = json });
    }
}
