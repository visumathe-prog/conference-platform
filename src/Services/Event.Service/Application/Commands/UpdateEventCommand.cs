using MediatR;
using Event.Service.Infrastructure.Repositories;

namespace Event.Service.Application.Commands;

public class UpdateEventCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
}

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, bool>
{
    private readonly IEventRepository _repository;

    public UpdateEventCommandHandler(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _repository.GetByIdAsync(request.Id);
        if (eventEntity is null) return false;

        eventEntity.Update(request.Title, request.Description, request.Date, request.Location, request.AvailableSeats, request.Price);
        await _repository.UpdateAsync(eventEntity);
        await _repository.SaveChangesAsync();

        return true;
    }
}
