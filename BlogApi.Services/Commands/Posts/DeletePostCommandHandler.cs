using BlogApi.Repository;
using MediatR;

namespace BlogApi.Services.Commands.Posts;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
{
    private readonly IPostRepository _repository;

    public DeletePostCommandHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetByIdAsync(request.id);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with ID {request.id} not found.");
        }

        await _repository.DeleteAsync(request.id);
        return Unit.Value; //  `Unit.Value` signify a void return type
    }
}