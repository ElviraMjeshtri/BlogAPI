using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using BlogApi.DTOs;
using BlogApi.Models;
using BlogApi.Services.Commands.Posts;
using BlogApi.Services.Queries.Posts;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;
using NSubstitute;


namespace BlogApi.Tests;

public class PostEndpointsTests : 
    IClassFixture<WebApplicationFactory<IApiMarker>>,
    IAsyncLifetime
{
    
    private readonly WebApplicationFactory<IApiMarker> _factory;
    private readonly HttpClient _httpClient;
    private readonly IMediator _mediatorMock;
  public PostEndpointsTests(WebApplicationFactory<IApiMarker> factory)
    {
        _mediatorMock = Substitute.For<IMediator>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the real IMediator with the mock
                services.AddSingleton(_mediatorMock);

                // Add a test authentication handler
                services.AddAuthentication("TestAuth")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });
            });

            builder.ConfigureAppConfiguration(config =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["JwtSettings:SecretKey"] = "ThisIsASecretKeyForJwt1234567890",
                    ["JwtSettings:Issuer"] = "BlogApi",
                    ["JwtSettings:Audience"] = "BlogApiAudience"
                });
            });
        });

        _httpClient = _factory.CreateClient();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");
    }

    [Fact]
    public async Task CreatePost_ReturnsCreated_WhenDataIsValid()
    {
        // Arrange
        var createPostDto = new CreatePostDto
        {
            Title = "Test Title",
            Content = "Test Content",
            FriendlyUrl = "test-url",
        };

        var createdPost = new PostDto
        {
            Id = 1,
            Title = createPostDto.Title,
            Content = createPostDto.Content,
            FriendlyUrl = createPostDto.FriendlyUrl
        };

        _mediatorMock.Send(Arg.Any<CreatePostCommand>()).Returns(createdPost);

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/posts", createPostDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responsePost = await response.Content.ReadFromJsonAsync<PostDto>();
        responsePost.Should().BeEquivalentTo(createdPost);
        response.Headers.Location.Should().Be($"api/posts/{createdPost.Id}");
    }
    [Fact]
    public async Task CreatePost_ReturnsBadRequest_WhenDataIsInvalid()
    {
        // Arrange
        var createPostDto = new CreatePostDto
        {
            Title = "", // Invalid title
            Content = "Test Content",
            FriendlyUrl = "test-url",
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/posts", createPostDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationErrors = await response.Content.ReadFromJsonAsync<JsonElement>();
        validationErrors.Should().NotBeNull();
    }
    
    [Fact]
    public async Task GetPosts_ReturnsOk_WhenAuthorizedAsUser()
    {
        // Arrange
        var posts = new List<PostDto>
        {
            new PostDto { Id = 1, Title = "Post 1", Content = "Content 1", FriendlyUrl = "post-1" },
            new PostDto { Id = 2, Title = "Post 2", Content = "Content 2", FriendlyUrl = "post-2" }
        };

        _mediatorMock.Send(Arg.Any<GetPostsQuery>()).Returns(posts);

        // Act
        var response = await _httpClient.GetAsync("/api/posts?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responsePosts = await response.Content.ReadFromJsonAsync<List<PostDto>>();
        responsePosts.Should().BeEquivalentTo(posts);
    }
    
    [Fact]
    public async Task GetPostById_ReturnsOk_WhenAuthorizedAsUser()
    {
        // Arrange
        var postId = 1;

        var postDto = new PostDto
        {
            Id = postId,
            Title = "Sample Title",
            Content = "Sample Content",
            FriendlyUrl = "sample-title",
            DateCreated = DateTime.UtcNow,
            CreatedBy = "Admin"
        };

        _mediatorMock.Send(Arg.Any<GetPostByIdQuery>()).Returns(postDto);

        // Act
        var response = await _httpClient.GetAsync($"/api/posts/{postId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responsePost = await response.Content.ReadFromJsonAsync<PostDto>();
        responsePost.Should().BeEquivalentTo(postDto);
    }
    
    [Fact]
    public async Task UpdatePost_ReturnsOk_WhenAuthorizedAsAdmin()
    {
        // Arrange
        var updatePostDto = new UpdatePostDto
        {
            Title = "Updated Title",
            Content = "Updated Content",
            FriendlyUrl = "updated-url"
        };

        var updatedPost = new PostDto
        {
            Id = 1,
            Title = updatePostDto.Title,
            Content = updatePostDto.Content,
            FriendlyUrl = updatePostDto.FriendlyUrl,
            DateCreated = DateTime.UtcNow,
            CreatedBy = "Admin"
        };

        _mediatorMock.Send(Arg.Any<UpdatePostCommand>()).Returns(updatedPost);

        // Act
        var response = await _httpClient.PutAsJsonAsync("/api/posts/1", updatePostDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responsePost = await response.Content.ReadFromJsonAsync<PostDto>();
        responsePost.Should().BeEquivalentTo(updatedPost);
    }
    
    [Fact]
    public async Task DeletePost_ReturnsNoContent_WhenAuthorizedAsAdmin()
    {
        // Arrange
        _mediatorMock.Send(Arg.Any<DeletePostCommand>()).Returns(Unit.Value);

        // Act
        var response = await _httpClient.DeleteAsync("/api/posts/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task ImportPosts_ReturnsOk_WhenAuthorizedAsAdmin()
    {
        // Arrange
        var csvUrl = "https://fleetcor-cvp.s3.eu-central-1.amazonaws.com/blog-posts.csv";
        var payload = new { csvUrl };

        // Mock mediator response
        _mediatorMock.Send(Arg.Any<ImportPostsFromCsvCommand>()).Returns(Unit.Value);

        // Add Authorization Header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "validAdminToken");

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/posts/import", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var message = await response.Content.ReadAsStringAsync();
        message.Should().Be("\"Import job completed successfully.\"");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;
    
}
