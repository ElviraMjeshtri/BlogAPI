using System.Net;
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
    public async Task Handle_ShouldReturnSuccessResult_WhenPostExists()
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
        Assert.True(result.IsSuccess); // ✅ Should be a successful result
        Assert.Equal(HttpStatusCode.OK, result.StatusCode); // ✅ Should return HTTP 200
        Assert.NotNull(result.Value);
        Assert.Equal(post.Id, result.Value.Id);
        Assert.Equal(post.Title, result.Value.Title);
        Assert.Equal(post.Content, result.Value.Content);
        Assert.Equal(post.FriendlyUrl, result.Value.FriendlyUrl);
        Assert.Equal(post.DateCreated, result.Value.DateCreated);
        Assert.Equal(post.CreatedBy, result.Value.CreatedBy);

        await _repository.Received(1).GetByIdAsync(postId);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenPostDoesNotExist()
    {
        // Arrange
        var postId = 1;
        _repository.GetByIdAsync(postId).Returns((Post)null);

        var query = new GetPostByIdQuery(postId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess); // ✅ Should be a failure result
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode); // ✅ Should return 404
        Assert.Equal($"Post with ID {postId} not found.", result.ErrorMessage);
        Assert.Null(result.Value); // ✅ Value should be null

        await _repository.Received(1).GetByIdAsync(postId);
    }
}
