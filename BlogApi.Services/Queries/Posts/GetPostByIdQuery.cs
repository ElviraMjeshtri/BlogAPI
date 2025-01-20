using BlogApi.DTOs;
using MediatR;

namespace BlogApi.Services.Queries.Posts;

public record GetPostByIdQuery(int PostId) : IRequest<PostDto>;