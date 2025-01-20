using BlogApi.data;
using BlogApi.Models;
using BlogApi.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Tests;

public class PostRepositoryTests
{
    private readonly DbContextOptions<BlogDbContext> _dbContextOptions = new DbContextOptionsBuilder<BlogDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

    [Fact]
    public async Task AddAsync_ShouldAddPostToDatabase()
    {
        // Arrange
        var dbContext = new BlogDbContext(_dbContextOptions);
        var repository = new PostRepository(dbContext);

        var post = new Post
        {
            Title = "Test Title",
            FriendlyUrl = "test-title",
            Content = "Test Content",
            DateCreated = System.DateTime.UtcNow,
            CreatedBy = "Admin"
        };

        // Act
        var result = await repository.AddAsync(post);

        // Assert
        result.Should().NotBeNull();
        (await dbContext.Posts.CountAsync()).Should().Be(1);
    }
}