using System.Globalization;
using System.Net;
using BlogApi.DTOs;
using BlogApi.Services;
using CsvHelper;
using CsvHelper.Configuration;

public class PostImportJob
{
    private readonly IPostService _postService;

    public PostImportJob(IPostService postService)
    {
        _postService = postService;
    }

    public async Task ImportPostsAsync(string csvUrl)
    {
        using var client = new WebClient();
        var csvContent = await client.DownloadStringTaskAsync(csvUrl);

        using var reader = new StringReader(csvContent);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null, // Ignore missing headers
            MissingFieldFound = null // Ignore missing fields
        };
        using var csv = new CsvReader(reader, config);

        // Map CSV headers to DTO properties
        csv.Context.RegisterClassMap<CreatePostDtoMap>();
        var posts = csv.GetRecords<CreatePostDto>().ToList();
        foreach (var postDto in posts)
        {
            try
            {
                await _postService.CreatePostAsync(postDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to import post with Title: {postDto.Title}. Error: {ex.Message}");
            }
        }
    }
}

// Define a class map for header mapping
public sealed class CreatePostDtoMap : ClassMap<CreatePostDto>
{
    public CreatePostDtoMap()
    {
        Map(m => m.Title).Name("title"); // TODO study the Map library here 
        Map(m => m.FriendlyUrl).Name("friendlyUrl");
        Map(m => m.Content).Name("content");
    }
}