using BlogApi.Models;

namespace BlogApi.Repository;

public interface IPostRepository
{
    Task<Post> AddAsync(Post post);
    Task<Post> UpdateAsync(Post post);
    Task DeleteAsync(int id);
    Task<Post> GetByIdAsync(int id);
    Task<IEnumerable<Post>> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<bool> FriendlyUrlExistsAsync(string friendlyUrl);
}