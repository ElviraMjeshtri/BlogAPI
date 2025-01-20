using BlogApi.DTOs;
using BlogApi.Repository;
using MediatR;

namespace BlogApi.Services.Queries.Posts;

public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, IEnumerable<PostDto>>
{
    private readonly IPostRepository _repository;

    public GetPostsQueryHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PostDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        // Fetch paginated posts
        var posts = await _repository.GetPaginatedAsync(
            request.pageNumber,
            request.pageSize);
        //map to Dto and return
        return posts.Select(post => new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            FriendlyUrl = post.FriendlyUrl,
            Content = post.Content,
            DateCreated = post.DateCreated,
            CreatedBy = post.CreatedBy
        });
    }
}