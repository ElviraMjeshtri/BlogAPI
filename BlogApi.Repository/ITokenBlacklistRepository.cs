namespace BlogApi.Repository;

public interface ITokenBlacklistRepository
{
    Task AddToBlacklistAsync(string token);
    Task<bool> IsTokenBlacklisted(string token);
}