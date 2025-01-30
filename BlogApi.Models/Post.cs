namespace BlogApi.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string FriendlyUrl { get; set; }
    public string Content { get; set; }
    public DateTime DateCreated { get; set; }
    public string CreatedBy { get; set; }
}