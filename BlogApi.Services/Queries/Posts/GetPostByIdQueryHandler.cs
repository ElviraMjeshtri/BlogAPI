using System.Net;
using MediatR;
using BlogApi.DTOs;
using BlogApi.Repository;
using BlogApi.Services.Commands;
using BlogApi.Services.Queries.Posts;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, Result<PostDto>>
{
    private readonly IPostRepository _repository;

    public GetPostByIdQueryHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PostDto>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetByIdAsync(request.PostId);
        if (post == null)
        {
            return Result<PostDto>.Failure(HttpStatusCode.NotFound, $"Post with ID {request.PostId} not found.");
        }

        var postDto = new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            FriendlyUrl = post.FriendlyUrl,
            DateCreated = post.DateCreated,
            CreatedBy = post.CreatedBy
        };

        return Result<PostDto>.Success(postDto);
    }
}