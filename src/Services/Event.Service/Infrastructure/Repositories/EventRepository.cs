using Microsoft.EntityFrameworkCore;
using Event.Service.Domain.Entities;
using Event.Service.Infrastructure.Data;

namespace Event.Service.Infrastructure.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id);
    Task AddAsync(Event eventEntity);
    Task UpdateAsync(Event eventEntity);
    Task DeleteAsync(Guid id);
    Task<int> SaveChangesAsync();
}

public class EventRepository : IEventRepository
{
    private readonly EventDbContext _context;

    public EventRepository(EventDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _context.Events.FindAsync(id);
    }

    public async Task AddAsync(Event eventEntity)
    {
        await _context.Events.AddAsync(eventEntity);
    }

    public Task UpdateAsync(Event eventEntity)
    {
        _context.Entry(eventEntity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var eventEntity = await GetByIdAsync(id);
        if (eventEntity is not null)
            _context.Events.Remove(eventEntity);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
