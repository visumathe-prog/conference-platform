using Registration.Service.Models;

namespace Registration.Service.Services;

public interface IRegistrationService
{
    Task<bool> RegisterAsync(Guid eventId, Guid userId, string ticketType);
    Task<List<UserTicket>> GetUserRegistrationsAsync(Guid userId);
}
