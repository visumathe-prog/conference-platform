using MediatR;
using Event.Service.Infrastructure.Repositories;

namespace Event.Service.Application.Commands;

public class DeleteEventCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, bool>
{
    private readonly IEventRepository _repository;

    public DeleteEventCommandHandler(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.Id);
        await _repository.SaveChangesAsync();
        return true;
    }
}
