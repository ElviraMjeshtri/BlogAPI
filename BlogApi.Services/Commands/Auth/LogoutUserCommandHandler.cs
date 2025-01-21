using BlogApi.Repository;
using MediatR;

namespace BlogApi.Services.Commands.Auth;

public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand>
{
    private readonly ITokenBlacklistRepository _blacklistRepository;

    public LogoutUserCommandHandler(ITokenBlacklistRepository blacklistRepository)
    {
        _blacklistRepository = blacklistRepository;
    }

    public async Task<Unit> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await _blacklistRepository.AddToBlacklistAsync(request.Token);
        return Unit.Value;
    }
}