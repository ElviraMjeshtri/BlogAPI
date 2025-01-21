using BlogApi.DTOs.Auth;
using BlogApi.Models;
using BlogApi.Repository;
using MediatR;

namespace BlogApi.Services.Commands.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly AuthService _tokenServices;

    public RegisterUserCommandHandler(IUserRepository userRepository, AuthService tokenServices)
    {
        _userRepository = userRepository;
        _tokenServices = tokenServices;
    }

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _userRepository.GetByUsernameAsync(request.RegisterUserDto.Username);
        if (userExists != null)
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.RegisterUserDto.Password);
        var newUser = new User
        {
            Username = request.RegisterUserDto.Username,
            PasswordHash = hashedPassword,
            Role = request.RegisterUserDto.Role
        };

        await _userRepository.AddUserAsync(newUser);

        var token = _tokenServices.GenerateJwtToken(newUser);
        return new AuthResponseDto { Token = token, Role = newUser.Role };
    }
}