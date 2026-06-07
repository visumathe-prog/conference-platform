using MediatR;
using Event.Service.Domain.Entities;
using Event.Service.Infrastructure.Repositories;

namespace Event.Service.Application.Commands;

public class CreateEventCommand : IRequest<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
}

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventRepository _repository;

    public CreateEventCommandHandler(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = new Event(
            request.Title,
            request.Description,
            request.Date,
            request.Location,
            request.AvailableSeats,
            request.Price
        );

        await _repository.AddAsync(eventEntity);
        await _repository.SaveChangesAsync();

        return eventEntity.Id;
    }
}
