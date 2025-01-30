using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services.Commands;
using BlogApi.Services.Commands.Posts;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace BlogApi.Tests.Commands.Posts;

public class DeletePostCommandHandlerTests
{
    private readonly IPostRepository _postRepository;
    private readonly DeletePostCommandHandler _handler;

    public DeletePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new DeletePostCommandHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_ShouldDeletePost_WhenPostExists()
    {
        // Arrange
        var postId = 1;
        var post = new Post
        {
            Id = postId,
            Title = "Test Post",
            Content = "Test Content",
            FriendlyUrl = "test-post",
            DateCreated = DateTime.UtcNow,
            CreatedBy = "Admin"
        };

        _postRepository.GetByIdAsync(postId).Returns(post);

        var command = new DeletePostCommand(postId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().BeEquivalentTo(Result<Unit>.Success(Unit.Value));
        await _postRepository.Received(1).GetByIdAsync(postId);
        await _postRepository.Received(1).DeleteAsync(postId);
    }

}