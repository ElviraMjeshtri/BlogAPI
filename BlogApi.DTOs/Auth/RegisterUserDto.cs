namespace BlogApi.DTOs.Auth;

public class RegisterUserDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } // Admin or User
}