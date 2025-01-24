using BlogApi.Models;

namespace BlogApi.Services;

public interface IAuthService
{
    public bool VerifyPassword(string password, string passwordHash);

    public string GenerateJwtToken(User user);

}