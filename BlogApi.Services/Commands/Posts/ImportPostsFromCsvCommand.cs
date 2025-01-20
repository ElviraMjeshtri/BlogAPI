using MediatR;

namespace BlogApi.Services.Commands.Posts;

public record ImportPostsFromCsvCommand (string CsvUrl) : IRequest;