using BlogApi.Models;
using CsvHelper.Configuration;

namespace BlogApi.DTOs.Mappers;


public class PostMap : ClassMap<Post>
{
    public PostMap()
    {
        Map(m => m.Id).Name("id");
        Map(m => m.Title).Name("title");
        Map(m => m.FriendlyUrl).Name("friendlyUrl");
        Map(m => m.Content).Name("content");
    }
}
