using BlogApi.DTOs;
using BlogApi.Models;
using BlogApi.Repository;

namespace BlogApi.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;

    public PostService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto)
    {
        //Check for duplicate FriendUrl
        var friendlyUrlExists = await _postRepository.FriendlyUrlExistsAsync(createPostDto.FriendlyUrl);
        if (friendlyUrlExists)
        {
            throw new SystemException("The friendly url already exist.");
        }

        //Map data 
        //TODO check if model to dto mapping can be done diffrently
        var post = new Post
        {
            Title = createPostDto.Title,
            Content = createPostDto.Content,
            FriendlyUrl = createPostDto.FriendlyUrl,
            DateCreated = DateTime.UtcNow,
            CreatedBy = "Admin" // TODO Replace with dynamic user context if available
        };
        // save to database
        var createdPost = await _postRepository.AddAsync(post);

        //map to dto and return

        return new PostDto
        {
            Id = createdPost.Id,
            DateCreated = createdPost.DateCreated,
            CreatedBy = createdPost.CreatedBy,
            Title = createPostDto.Title,
            Content = createPostDto.Content,
            FriendlyUrl = createPostDto.FriendlyUrl,
        };
    }

    public async Task<PostDto> UpdatePostAsync(int id, UpdatePostDto updatePostDto)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            throw new SystemException("The post does not exist.");
        }

        //Update properties
        post.Title = updatePostDto.Title;
        post.Content = updatePostDto.Content;
        post.FriendlyUrl = updatePostDto.FriendlyUrl;

        //Save to database
        var updatedPost = await _postRepository.UpdateAsync(post);

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

    public async Task DeletePostAsync(int id)
    {
        // ensure the post exist
        var post = _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            throw new SystemException("The post not found");
        }

        // delete from database
        await _postRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<PostDto>> GetPostsAsync(int pageNumber, int pageSize)
    {
        // Fetch paginated posts
        var posts = await _postRepository.GetPaginatedAsync(pageNumber, pageSize);
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

    public async Task<PostDto> GetPostAsync(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            FriendlyUrl = post.FriendlyUrl,
            Content = post.Content,
            DateCreated = post.DateCreated,
            CreatedBy = post.CreatedBy
        };
    }
}