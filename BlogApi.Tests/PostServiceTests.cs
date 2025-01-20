using BlogApi.DTOs;
using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services;
using FluentAssertions;
using NSubstitute;

namespace BlogApi.Tests;

public class PostServiceTests
{
    
    private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
    private readonly PostService _postService;

    public PostServiceTests()
    {
        _postService = new PostService(_postRepository);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldReturnPostDto_WhenInputIsValid()
    {
        // Arrange
        var dto = new CreatePostDto
        {
            Title = "Test Title",
            FriendlyUrl = "test-title",
            Content = "Test Content"
        };

        var createdPost = new Post
        {
            Id = 1,
            Title = dto.Title,
            FriendlyUrl = dto.FriendlyUrl,
            Content = dto.Content,
            DateCreated = System.DateTime.UtcNow,
            CreatedBy = "Admin"
        };

        _postRepository.AddAsync(Arg.Any<Post>()).Returns(createdPost);

        // Act
        var result = await _postService.CreatePostAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.FriendlyUrl.Should().Be(dto.FriendlyUrl);
        result.Content.Should().Be(dto.Content);
    }
}