using BlogApi.DTOs;
using BlogApi.Services.Commands;
using MediatR;

namespace BlogApi.Services.Queries.Posts;

public record GetPostByIdQuery(int PostId) : IRequest<Result<PostDto>>;