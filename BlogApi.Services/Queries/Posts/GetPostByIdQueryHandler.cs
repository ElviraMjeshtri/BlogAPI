namespace BlogApi.Services.Queries.Posts;

using MediatR;
using BlogApi.DTOs;
using BlogApi.Repository;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostDto>
{
    private readonly IPostRepository _repository;

    public GetPostByIdQueryHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<PostDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetByIdAsync(request.PostId);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with ID {request.PostId} not found.");
        }
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            FriendlyUrl = post.FriendlyUrl,
            DateCreated = post.DateCreated,
            CreatedBy = post.CreatedBy
        };
    }
}
