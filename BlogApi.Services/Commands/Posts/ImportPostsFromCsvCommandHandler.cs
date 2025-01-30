using System.Globalization;
using BlogApi.DTOs.Mappers;
using BlogApi.Models;
using BlogApi.Repository;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BlogApi.Services.Commands.Posts;

public class ImportPostsFromCsvCommandHandler : IRequestHandler<ImportPostsFromCsvCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ImportPostsFromCsvCommandHandler(IPostRepository postRepository,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)

    {
        _postRepository = postRepository;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Unit> Handle(ImportPostsFromCsvCommand request, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(request.CsvUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to fetch CSV from {request.CsvUrl}, status code: {response.StatusCode}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        csv.Context.RegisterClassMap<PostMap>();
        var posts = csv.GetRecords<Post>().ToList();
        var username = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Unknown";

        foreach (var post in posts)
        {
            post.CreatedBy = username;
            post.DateCreated = DateTime.UtcNow;
            await _postRepository.AddAsync(post);
        }

        return Unit.Value;
    }
}