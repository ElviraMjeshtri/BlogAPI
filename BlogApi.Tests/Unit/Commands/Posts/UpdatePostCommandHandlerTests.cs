namespace BlogApi.Tests.Commands.Posts;

using System;
using System.Threading;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services.Commands.Posts;
using FluentAssertions;
using NSubstitute;
using Xunit;


public class UpdatePostCommandHandlerTests
{
    private readonly IPostRepository _postRepository;
    private readonly UpdatePostCommandHandler _handler;

    public UpdatePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new UpdatePostCommandHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_ShouldUpdatePost_WhenPostExists()
    {
        // Arrange
        var postId = 1;
        var existingPost = new Post
        {
            Id = postId,
            Title = "Old Title",
            Content = "Old Content",
            FriendlyUrl = "old-url",
            DateCreated = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "Admin"
        };

        var updatePostDto = new UpdatePostDto
        {
            Title = "New Title",
            Content = "New Content",
            FriendlyUrl = "new-url"
        };

        var updatedPost = new Post
        {
            Id = postId,
            Title = updatePostDto.Title,
            Content = updatePostDto.Content,
            FriendlyUrl = updatePostDto.FriendlyUrl,
            DateCreated = existingPost.DateCreated,
            CreatedBy = existingPost.CreatedBy
        };

        _postRepository.GetByIdAsync(postId).Returns(Task.FromResult<Post?>(existingPost));
        _postRepository.UpdateAsync(Arg.Any<Post>()).Returns(Task.FromResult(updatedPost));

        var command = new UpdatePostCommand(postId, updatePostDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(updatePostDto.Title);
        result.Content.Should().Be(updatePostDto.Content);
        result.FriendlyUrl.Should().Be(updatePostDto.FriendlyUrl);

        await _postRepository.Received(1).GetByIdAsync(postId);
        await _postRepository.Received(1).UpdateAsync(Arg.Is<Post>(p =>
            p.Id == postId &&
            p.Title == updatePostDto.Title &&
            p.Content == updatePostDto.Content &&
            p.FriendlyUrl == updatePostDto.FriendlyUrl));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPostDoesNotExist()
    {
        // Arrange
        var postId = 1;

        var updatePostDto = new UpdatePostDto
        {
            Title = "New Title",
            Content = "New Content",
            FriendlyUrl = "new-url"
        };

        _postRepository.GetByIdAsync(postId).Returns(Task.FromResult<Post?>(null));

        var command = new UpdatePostCommand(postId, updatePostDto);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SystemException>()
            .WithMessage("The post does not exist.");
        await _postRepository.Received(1).GetByIdAsync(postId);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>());
    }
}
