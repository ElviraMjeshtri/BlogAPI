namespace BlogApi.Models;

public class BlacklistedToken
{
    public string Token { get; set; }
    public DateTime BlacklistedAt { get; set; }
}