using System.Globalization;
using BlogApi.Models;
using BlogApi.Repository;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;

namespace BlogApi.Services.Commands.Posts;

public class ImportPostsFromCsvCommandHandler : IRequestHandler<ImportPostsFromCsvCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly HttpClient _httpClient;

    public ImportPostsFromCsvCommandHandler(IPostRepository postRepository, HttpClient httpClient)
    {
        _postRepository = postRepository;
        _httpClient = httpClient;
    }

    public async Task<Unit> Handle(ImportPostsFromCsvCommand request, CancellationToken cancellationToken)
    {
        var csvStream = await _httpClient.GetStreamAsync(request.CsvUrl);

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        var posts = csv.GetRecords<Post>().ToList();

        foreach (var post in posts)
        {
            await _postRepository.AddAsync(post);
        }

        return Unit.Value;
    }
}

