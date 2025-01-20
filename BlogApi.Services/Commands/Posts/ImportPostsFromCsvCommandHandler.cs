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

    public ImportPostsFromCsvCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Unit> Handle(ImportPostsFromCsvCommand request, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var csvStream = await httpClient.GetStreamAsync(request.CsvUrl);

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        var posts = csv.GetRecords<Post>().ToList();

        foreach (var post in posts)
        {
            // Optionally add validation or mapping logic here
            await _postRepository.AddAsync(post);
        }

        return Unit.Value; // Indicates the operation completed successfully
    }
}
