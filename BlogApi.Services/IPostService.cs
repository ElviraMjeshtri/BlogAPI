using BlogApi.DTOs;

namespace BlogApi.Services;

public interface IPostService
{
    Task<PostDto> CreatePostAsync(CreatePostDto createPostDto);
    Task<PostDto> UpdatePostAsync(int id, UpdatePostDto updatePostDto);
    Task DeletePostAsync(int id);
    Task<IEnumerable<PostDto>> GetPostsAsync(int pageNumber, int pageSize);

    Task<PostDto> GetPostAsync(int id);
}