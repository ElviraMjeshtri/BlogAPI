using System.Net;
using BlogApi.Repository;
using BlogApi.Services.Commands;
using BlogApi.Services.Commands.Posts;
using MediatR;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, Result<Unit>>
{
    private readonly IPostRepository _repository;

    public DeletePostCommandHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Unit>> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetByIdAsync(request.id);
        if (post == null)
        {
            return Result<Unit>.Failure(HttpStatusCode.NotFound, $"Post with ID {request.id} not found.");
        }

        await _repository.DeleteAsync(request.id);
        return Result<Unit>.Success(Unit.Value);
    }
}