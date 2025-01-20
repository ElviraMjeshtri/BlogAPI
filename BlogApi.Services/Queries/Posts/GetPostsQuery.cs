using BlogApi.DTOs;
using MediatR;

namespace BlogApi.Services.Queries.Posts;

public record GetPostsQuery(int pageNumber, int pageSize) : IRequest<IEnumerable<PostDto>>;