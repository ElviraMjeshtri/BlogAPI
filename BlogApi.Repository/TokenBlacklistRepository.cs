namespace BlogApi.Repository;

public class TokenBlacklistRepository : ITokenBlacklistRepository
{
    public Task AddToBlacklistAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsTokenBlacklisted(string token)
    {
        throw new NotImplementedException();
    }
}