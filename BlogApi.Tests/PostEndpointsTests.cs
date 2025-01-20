using System.Net;
using BlogApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;


namespace BlogApi.Tests;

public class PostEndpointsTests : 
    IClassFixture<WebApplicationFactory<IApiMarker>>,
    IAsyncLifetime
{
    
    private readonly WebApplicationFactory<IApiMarker> _factory;
    private readonly List<int> _createdIds = new ();
    private static readonly Random random = new Random();

    public PostEndpointsTests(WebApplicationFactory<IApiMarker> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task CreatePost_CreatesPost_WhenDataIsCorrect()
    {
        //arrange 
        var httpClient = _factory.CreateClient();
        var post = GeneratePost();
        //act 
        var result = await httpClient.PostAsJsonAsync("/api/posts", post);
        _createdIds.Add(post.Id);
        var createdPost = await result.Content.ReadFromJsonAsync<Post>();
        //assert
        result.Headers.Location.Should().Be($"api/posts/{createdPost.Id}");
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdPost.Should().BeEquivalentTo(post, options => options
                .Excluding(x => x.Id)               // Ignore Id
                .Excluding(x => x.DateCreated)      // Ignore DateCreated
        );

    }

    private Post GeneratePost()
    {
        return new Post
        {
            Title = "This is a post title",
            Content = "This is a test content",
            FriendlyUrl = random.Next(100).ToString(),
            CreatedBy = "Admin"
        };
    }

    public Task InitializeAsync() => Task.CompletedTask;

    // Delete created books for testing reason
    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();
        foreach (var id in _createdIds)
        {
            var response = await httpClient.DeleteAsync($"/api/posts/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to delete post with id {id}. " +
                                  $"Status code: {response.StatusCode}." +
                                  $" Response: {responseBody}");
            }
            else
            {
                Console.WriteLine($"Successfully deleted podt with id {id}.");
            }
        }
        _createdIds.Clear();
    }
}