using BlogApi.DTOs;
using BlogApi.Models;
using BlogApi.Repository;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BlogApi.Services.Commands.Posts;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IPostRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreatePostCommandHandler(IPostRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var username = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Unknown";

        //Check for duplicate FriendUrl
        var friendlyUrlExists = await _repository.FriendlyUrlExistsAsync(request.CreatePostDto.FriendlyUrl);
        if (friendlyUrlExists)
        {
            throw new SystemException("The friendly url already exists.");
        }

        var post = new Post
        {
            Title = request.CreatePostDto.Title,
            Content = request.CreatePostDto.Content,
            FriendlyUrl = request.CreatePostDto.FriendlyUrl,
            DateCreated = DateTime.UtcNow,
            CreatedBy = username
        };

        var createdPost = await _repository.AddAsync(post);

        return new PostDto
        {
            Id = createdPost.Id,
            Title = createdPost.Title,
            Content = createdPost.Content,
            FriendlyUrl = createdPost.FriendlyUrl,
            DateCreated = createdPost.DateCreated,
            CreatedBy = createdPost.CreatedBy
        };
    }
}