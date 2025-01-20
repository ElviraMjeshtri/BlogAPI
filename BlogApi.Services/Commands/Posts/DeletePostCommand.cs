using MediatR;

namespace BlogApi.Services.Commands.Posts;

public record DeletePostCommand(int id) : IRequest;
