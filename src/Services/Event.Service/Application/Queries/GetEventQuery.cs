using MediatR;
using Event.Service.Domain.Entities;
using Event.Service.Infrastructure.Repositories;

namespace Event.Service.Application.Queries;

public class GetEventQuery : IRequest<Event?>
{
    public Guid Id { get; set; }
}

public class GetEventQueryHandler : IRequestHandler<GetEventQuery, Event?>
{
    private readonly IEventRepository _repository;

    public GetEventQueryHandler(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<Event?> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id);
    }
}
```

---

Event.Service/Application/Queries/GetEventsListQuery.cs

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using Event.Service.Infrastructure.Data;

namespace Event.Service.Application.Queries;

public class GetEventsListQuery : IRequest<List<EventDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class EventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
}

public class GetEventsListQueryHandler : IRequestHandler<GetEventsListQuery, List<EventDto>>
{
    private readonly EventDbContext _context;

    public GetEventsListQueryHandler(EventDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventDto>> Handle(GetEventsListQuery request, CancellationToken cancellationToken)
    {
        return await _context.Events
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location,
                AvailableSeats = e.AvailableSeats,
                Price = e.Price
            })
            .ToListAsync(cancellationToken);
    }
}
