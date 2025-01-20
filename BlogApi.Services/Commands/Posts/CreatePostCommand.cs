using BlogApi.DTOs;
using MediatR;

namespace BlogApi.Services.Commands.Posts;

public record CreatePostCommand(CreatePostDto CreatePostDto) : IRequest<PostDto>;