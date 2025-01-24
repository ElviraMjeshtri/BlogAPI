using BlogApi.DTOs.Auth;
using BlogApi.Repository;
using MediatR;

namespace BlogApi.Services.Commands.Auth;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public LoginUserCommandHandler(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var loginDto = request.LoginRequestDto;
        //check if user exists
        var user  =  await _userRepository.GetByUsernameAsync(loginDto.Username);
        if (user == null)
        {
            throw new Exception("Invalid username or password");
        }
        //verify password
        if (!_authService.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            throw new Exception("Invalid username or password");
        }
        
        // generate token
        var token = _authService.GenerateJwtToken(user);
        
        //return AuthResponseDto
        return new AuthResponseDto
        {
            Token = token,
            Role = user.Role

        };
    }
}