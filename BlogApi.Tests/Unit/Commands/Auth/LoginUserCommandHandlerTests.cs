using BlogApi.DTOs.Auth;
using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services;
using BlogApi.Services.Commands.Auth;
using NSubstitute;

namespace BlogApi.Tests.Commands.Auth;

public class LoginUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _authService = Substitute.For<IAuthService>();
        _handler = new LoginUserCommandHandler(_userRepository, _authService);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuthResponseDto_WhenCredentialsAreValid()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";
        var loginDto = new LoginRequestDto { Username = username, Password = password };
        var user = new User
        {
            Username = username,
            PasswordHash = "hashedpassword",
            Role = "User"
        };

        _userRepository.GetByUsernameAsync(username).Returns(user);
        _authService.VerifyPassword(password, user.PasswordHash).Returns(true);
        _authService.GenerateJwtToken(user).Returns("jwt-token");

        var command = new LoginUserCommand(loginDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("User", result.Role);

        await _userRepository.Received(1).GetByUsernameAsync(username);
        _authService.Received(1).VerifyPassword(password, user.PasswordHash);
        _authService.Received(1).GenerateJwtToken(user);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenUsernameIsInvalid()
    {
        // Arrange
        var username = "invaliduser";
        var password = "testpassword";
        var loginDto = new LoginRequestDto { Username = username, Password = password };

        _userRepository.GetByUsernameAsync(username).Returns((User)null);

        var command = new LoginUserCommand(loginDto);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        Assert.Equal("Invalid username or password", exception.Message);

        await _userRepository.Received(1).GetByUsernameAsync(username);
        _authService.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
        _authService.DidNotReceive().GenerateJwtToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenPasswordIsInvalid()
    {
        // Arrange
        var username = "testuser";
        var password = "invalidpassword";
        var loginDto = new LoginRequestDto { Username = username, Password = password };
        var user = new User
        {
            Username = username,
            PasswordHash = "hashedpassword",
            Role = "User"
        };

        _userRepository.GetByUsernameAsync(username).Returns(user);
        _authService.VerifyPassword(password, user.PasswordHash).Returns(false);

        var command = new LoginUserCommand(loginDto);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None)
        );

        Assert.Equal("Invalid username or password", exception.Message);

        await _userRepository.Received(1).GetByUsernameAsync(username);
        _authService.Received(1).VerifyPassword(password, user.PasswordHash);
        _authService.DidNotReceive().GenerateJwtToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenCommandIsNull()
    {
        // Arrange
        LoginUserCommand command = null;

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentNullException_WhenLoginRequestDtoIsNull()
    {
        // Arrange
        var command = new LoginUserCommand(null);

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );
    }
}