using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services.Queries.Posts;
using NSubstitute;

namespace BlogApi.Tests.Posts;

public class GetPostByIdQueryHandlerTests
{
    private readonly IPostRepository _repository;
    private readonly GetPostByIdQueryHandler _handler;

    public GetPostByIdQueryHandlerTests()
    {
        _repository = Substitute.For<IPostRepository>();
        _handler = new GetPostByIdQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnPostDto_WhenPostExists()
    {
        // Arrange
        var postId = 1;
        var post = new Post
        {
            Id = postId,
            Title = "Test Post",
            Content = "This is a test post.",
            FriendlyUrl = "test-post",
            DateCreated = DateTime.UtcNow,
            CreatedBy = "test-user"
        };

        _repository.GetByIdAsync(postId).Returns(post);

        var query = new GetPostByIdQuery(postId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(post.Id, result.Id);
        Assert.Equal(post.Title, result.Title);
        Assert.Equal(post.Content, result.Content);
        Assert.Equal(post.FriendlyUrl, result.FriendlyUrl);
        Assert.Equal(post.DateCreated, result.DateCreated);
        Assert.Equal(post.CreatedBy, result.CreatedBy);

        await _repository.Received(1).GetByIdAsync(postId);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenPostDoesNotExist()
    {
        // Arrange
        var postId = 1;
        _repository.GetByIdAsync(postId).Returns((Post)null);

        var query = new GetPostByIdQuery(postId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None)
        );

        Assert.Equal($"Post with ID {postId} not found.", exception.Message);

        await _repository.Received(1).GetByIdAsync(postId);
    }
}