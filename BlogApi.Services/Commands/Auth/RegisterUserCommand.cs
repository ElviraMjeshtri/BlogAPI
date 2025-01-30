using BlogApi.DTOs.Auth;
using MediatR;

namespace BlogApi.Services.Commands.Auth;

public record RegisterUserCommand(RegisterUserDto RegisterUserDto) : IRequest<AuthResponseDto>
{
}