using ClickHouse.Ado;
using Analytics.Service.Models;
using System.Data;

namespace Analytics.Service.Services;

public class ClickHouseService : IAnalyticsService
{
    private readonly string _connectionString;

    public ClickHouseService(IConfiguration configuration)
    {
        _connectionString = configuration["ClickHouse:ConnectionString"] ?? "Host=localhost;Port=8123;Database=analytics";
    }

    public async Task<EventStat> GetEventStatisticsAsync(Guid eventId)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                eventId,
                count() as totalRegistrations,
                sum(amount) as totalRevenue,
                count(distinct userId) as uniqueAttendees
            FROM registrations
            WHERE eventId = @eventId
            GROUP BY eventId";

        command.Parameters.Add(new ClickHouseParameter { ParameterName = "eventId", Value = eventId.ToString() });

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new EventStat
            {
                EventId = eventId,
                TotalRegistrations = reader.GetInt32(1),
                TotalRevenue = reader.GetDecimal(2),
                UniqueAttendees = reader.GetInt32(3)
            };
        }

        return new EventStat { EventId = eventId };
    }

    public async Task<List<EventStat>> GetTopEventsAsync(int limit)
    {
        var result = new List<EventStat>();

        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                eventId,
                count() as totalRegistrations,
                sum(amount) as totalRevenue
            FROM registrations
            GROUP BY eventId
            ORDER BY totalRegistrations DESC
            LIMIT @limit";

        command.Parameters.Add(new ClickHouseParameter { ParameterName = "limit", Value = limit });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new EventStat
            {
                EventId = Guid.Parse(reader.GetString(0)),
                TotalRegistrations = reader.GetInt32(1),
                TotalRevenue = reader.GetDecimal(2)
            });
        }

        return result;
    }

    public async Task<Dictionary<DateTime, int>> GetDailyRegistrationsAsync(int days)
    {
        var result = new Dictionary<DateTime, int>();

        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                toDate(createdAt) as date,
                count() as registrations
            FROM registrations
            WHERE createdAt >= today() - @days
            GROUP BY date
            ORDER BY date";

        command.Parameters.Add(new ClickHouseParameter { ParameterName = "days", Value = days });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetDateTime(0), reader.GetInt32(1));
        }

        return result;
    }

    public async Task RecordRegistrationEventAsync(RegistrationEvent registrationEvent)
    {
        using var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO registrations (eventId, userId, amount, createdAt)
            VALUES (@eventId, @userId, @amount, @createdAt)";

        command.Parameters.Add(new ClickHouseParameter { ParameterName = "eventId", Value = registrationEvent.EventId.ToString() });
        command.Parameters.Add(new ClickHouseParameter { ParameterName = "userId", Value = registrationEvent.UserId.ToString() });
        command.Parameters.Add(new ClickHouseParameter { ParameterName = "amount", Value = registrationEvent.Amount });
        command.Parameters.Add(new ClickHouseParameter { ParameterName = "createdAt", Value = registrationEvent.CreatedAt });

        await command.ExecuteNonQueryAsync();
    }
}
