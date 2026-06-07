using Microsoft.Extensions.Hosting;
using Confluent.Kafka;
using System.Text.Json;
using Notification.Service.Models;
using Notification.Service.Services;

namespace Notification.Service.Consumers;

public class RegistrationNotificationConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly INotificationService _notificationService;
    private readonly string _topic;

    public RegistrationNotificationConsumer(IConfiguration configuration, INotificationService notificationService)
    {
        _notificationService = notificationService;
        _topic = configuration["Kafka:Topic"] ?? "notification-events";
        
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "notification-group",
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
                var notification = JsonSerializer.Deserialize<RegistrationNotification>(result.Message.Value);
                
                if (notification is not null)
                {
                    var subject = "Registration Confirmed";
                    var body = $"Hello, you have successfully registered for the event. Certificate ID: {notification.CertificateId}";
                    await _notificationService.SendEmailAsync(notification.Email, subject, body);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        _consumer.Close();
    }
}
