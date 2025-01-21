using MediatR;

namespace BlogApi.Services.Commands.Auth;

public record LogoutUserCommand(string Token) : IRequest;