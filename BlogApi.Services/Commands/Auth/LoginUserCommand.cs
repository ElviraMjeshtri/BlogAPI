using BlogApi.DTOs.Auth;
using MediatR;

namespace BlogApi.Services.Commands.Auth;

public record LoginUserCommand (LoginRequestDto LoginRequestDto) : IRequest<AuthResponseDto>;
