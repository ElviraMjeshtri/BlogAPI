using BlogApi.DTOs;
using BlogApi.Repository;
using MediatR;

namespace BlogApi.Services.Commands.Posts;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, PostDto>
{
    private readonly IPostRepository _repository;

    public UpdatePostCommandHandler(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<PostDto> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetByIdAsync(request.id);
        if (post == null)
        {
            throw new SystemException("The post does not exist.");
        }
        //Update properties
        post.Title = request.updatePostDto.Title;
        post.Content = request.updatePostDto.Content;
        post.FriendlyUrl = request.updatePostDto.FriendlyUrl;
        
        //Save to database
        var updatedPost = await _repository.UpdateAsync(post);
        
        //Map to Dto and return
        return new PostDto
        {
            Id = updatedPost.Id,
            DateCreated = updatedPost.DateCreated,
            CreatedBy = updatedPost.CreatedBy,
            Title = updatedPost.Title,
            Content = updatedPost.Content,
            FriendlyUrl = updatedPost.FriendlyUrl
        };
    }
}