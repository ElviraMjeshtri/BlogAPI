namespace BlogApi.Tests.Commands.Posts;

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services.Commands.Posts;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

public class CreatePostCommandHandlerTests
{
    private readonly IPostRepository _postRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CreatePostCommandHandler _handler;

    public CreatePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        // Simulate logged-in user
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "TestUser")
            }, "TestAuth"))
        };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        _handler = new CreatePostCommandHandler(_postRepository, _httpContextAccessor);
    }

    [Fact]
    public async Task Handle_ShouldCreatePost_WhenDataIsValid()
    {
        // Arrange
        var createPostDto = new CreatePostDto
        {
            Title = "Test Post",
            Content = "Test Content",
            FriendlyUrl = "test-post"
        };

        var command = new CreatePostCommand(createPostDto);

        var createdPost = new Post
        {
            Id = 1,
            Title = createPostDto.Title,
            Content = createPostDto.Content,
            FriendlyUrl = createPostDto.FriendlyUrl,
            DateCreated = DateTime.UtcNow,
            CreatedBy = "TestUser"
        };

        _postRepository.FriendlyUrlExistsAsync(createPostDto.FriendlyUrl).Returns(false);
        _postRepository.AddAsync(Arg.Any<Post>()).Returns(createdPost);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(createPostDto.Title);
        result.Content.Should().Be(createPostDto.Content);
        result.FriendlyUrl.Should().Be(createPostDto.FriendlyUrl);
        result.CreatedBy.Should().Be("TestUser");

        await _postRepository.Received(1).FriendlyUrlExistsAsync(createPostDto.FriendlyUrl);
        await _postRepository.Received(1).AddAsync(Arg.Is<Post>(p =>
            p.Title == createPostDto.Title &&
            p.Content == createPostDto.Content &&
            p.FriendlyUrl == createPostDto.FriendlyUrl &&
            p.CreatedBy == "TestUser"));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenFriendlyUrlExists()
    {
        // Arrange
        var createPostDto = new CreatePostDto
        {
            Title = "Duplicate Post",
            Content = "Duplicate Content",
            FriendlyUrl = "duplicate-post"
        };

        var command = new CreatePostCommand(createPostDto);

        _postRepository.FriendlyUrlExistsAsync(createPostDto.FriendlyUrl).Returns(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SystemException>()
            .WithMessage("The friendly url already exists.");

        await _postRepository.Received(1).FriendlyUrlExistsAsync(createPostDto.FriendlyUrl);
        await _postRepository.DidNotReceive().AddAsync(Arg.Any<Post>());
    }
}
