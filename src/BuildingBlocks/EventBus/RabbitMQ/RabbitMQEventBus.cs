using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace BuildingBlocks.EventBus.RabbitMQ;

public class RabbitMQEventBus
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQEventBus(string connectionString)
    {
        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Publish<T>(T @event, string queueName)
    {
        _channel.QueueDeclare(queueName, durable: true, exclusive: false);
        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish("", queueName, body: body);
    }
}
