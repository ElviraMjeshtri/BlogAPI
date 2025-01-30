namespace BlogApi.DTOs;

public class CreatePostDto
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string FriendlyUrl { get; set; }
}