using BlogApi.data;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Repository;

public class DatabaseTokenBlacklistRepository: ITokenBlacklistRepository
{
    private readonly BlogDbContext _context;

    public DatabaseTokenBlacklistRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task AddToBlacklistAsync(string token)
    {
        _context.TokenBlacklist.Add(new BlacklistedToken
        {
            Token = token,
            BlacklistedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsTokenBlacklisted(string token)
    {
        return await _context.TokenBlacklist.AnyAsync(t => t.Token == token);
    }
}