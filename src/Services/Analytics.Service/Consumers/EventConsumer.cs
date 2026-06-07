using Microsoft.Extensions.Hosting;
using Confluent.Kafka;
using System.Text.Json;
using Analytics.Service.Models;
using Analytics.Service.Services;

namespace Analytics.Service.Consumers;

public class EventConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IAnalyticsService _analyticsService;
    private readonly string _topic;

    public EventConsumer(IConfiguration configuration, IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
        _topic = configuration["Kafka:Topic"] ?? "registration-events";
        
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "analytics-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                var registrationEvent = JsonSerializer.Deserialize<RegistrationEvent>(result.Message.Value);
                
                if (registrationEvent is not null)
                {
                    await _analyticsService.RecordRegistrationEventAsync(registrationEvent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error consuming message: {ex.Message}");
            }
        }

        _consumer.Close();
    }
}
