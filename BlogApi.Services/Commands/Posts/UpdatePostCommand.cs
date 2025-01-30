using BlogApi.DTOs;
using MediatR;

namespace BlogApi.Services.Commands.Posts;

public record UpdatePostCommand(int id, UpdatePostDto updatePostDto) : IRequest<PostDto>;