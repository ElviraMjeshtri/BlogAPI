using BlogApi.data;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Repository;

public class PostRepository : IPostRepository
{
    //TODO explain all the methods here 
    private readonly BlogDbContext _context;

    public PostRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Post> AddAsync(Post post)
    {
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<Post> UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task DeleteAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Post> GetByIdAsync(int id)
    {
        return await _context.Posts.FindAsync(id);
    }

    public async Task<IEnumerable<Post>> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        return await _context.Posts
            .OrderByDescending(p => p.DateCreated)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> FriendlyUrlExistsAsync(string friendlyUrl)
    {
        return await _context.Posts
            .AnyAsync(p => p.FriendlyUrl == friendlyUrl);
    }
}