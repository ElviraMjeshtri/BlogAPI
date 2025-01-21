using BlogApi.Models;

namespace BlogApi.Repository;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(String username);
    Task AddUserAsync(User user);
}