using BlogApi.DTOs;
using BlogApi.Models;
using BlogApi.Repository;
using MediatR;

namespace BlogApi.Services.Commands.Posts;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IPostRepository _repository;

    public CreatePostCommandHandler(IPostRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        //Check for duplicate FriendUrl
        var friendlyUrlExists = await _repository.FriendlyUrlExistsAsync(request.CreatePostDto.FriendlyUrl);
        if (friendlyUrlExists)
        {
            throw new SystemException("The friendly url already exist.");
        }
        
        var post = new Post
        {
            Title = request.CreatePostDto.Title,
            Content = request.CreatePostDto.Content,
            FriendlyUrl = request.CreatePostDto.FriendlyUrl,
            DateCreated = DateTime.UtcNow,
            CreatedBy = "Admin" // Replace with dynamic user info if available
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