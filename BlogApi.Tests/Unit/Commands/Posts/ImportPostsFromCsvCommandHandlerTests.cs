using System.Net;
using System.Security.Claims;
using System.Text;
using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services.Commands.Posts;
using BlogApi.Services.Helpers;
using NSubstitute;
using CsvHelper;

public class ImportPostsFromCsvCommandHandlerTests
{
    private readonly IPostRepository _postRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ImportPostsFromCsvCommandHandler _handler;
    private readonly HttpMessageHandler _mockHttpMessageHandler;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ImportPostsFromCsvCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();

        // Create the mock HttpMessageHandler
        _mockHttpMessageHandler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_mockHttpMessageHandler);

        // Mock IHttpClientFactory to return the HttpClient
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        // Initialize the handler
        _handler = new ImportPostsFromCsvCommandHandler(_postRepository, 
            _httpClientFactory,
            _httpContextAccessor);
    }

    [Fact]
    public async Task Handle_ShouldImportPostsFromCsv_WhenValidCsvProvided()
    {
        // Arrange
        var csvUrl = "http://example.com/posts.csv";
        var csvContent = "id,title,friendlyUrl,content\n1,Title1,Url1,Content1\n2,Title2,Url2,Content2";
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        csvStream.Position = 0;

        // Mock HttpClient
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(csvStream)
        };
        mockHttpMessageHandler.SetResponse(response);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        // Mock IHttpContextAccessor
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var claims = new[] { new Claim(ClaimTypes.Name, "TestUser") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = claimsPrincipal });

        var postRepository = Substitute.For<IPostRepository>();
        var handler = new ImportPostsFromCsvCommandHandler(postRepository, httpClientFactory, httpContextAccessor);

        // Act
        var command = new ImportPostsFromCsvCommand(csvUrl);
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await postRepository.Received(2).AddAsync(Arg.Any<Post>());
        await postRepository.Received(1).AddAsync(Arg.Is<Post>(p => p.Title == "Title1" && p.CreatedBy == "TestUser"));
        await postRepository.Received(1).AddAsync(Arg.Is<Post>(p => p.Title == "Title2" && p.CreatedBy == "TestUser"));
    }



    [Fact]
    public async Task Handle_ShouldThrowHttpRequestException_WhenHttpRequestFails()
    {
        // Arrange
        var csvUrl = "http://example.com/posts.csv";
        ((MockHttpMessageHandler)_mockHttpMessageHandler).SetException(new HttpRequestException("Request failed"));

        // Act & Assert
        var command = new ImportPostsFromCsvCommand(csvUrl);
        await Assert.ThrowsAsync<HttpRequestException>(() => _handler.Handle(command, CancellationToken.None));

        // Verify no posts were added to the repository
        await _postRepository.DidNotReceive().AddAsync(Arg.Any<Post>());
    }
    [Fact]
    public async Task Handle_ShouldNotAddPosts_WhenCsvIsEmpty()
    {
        // Arrange
        var csvUrl = "http://example.com/posts.csv";
        var csvContent = "id,title,friendlyUrl,content\n"; // Empty data after the header
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        csvStream.Position = 0; // Ensure the stream is at the beginning

        // Mock HttpClient
        var mockHttpMessageHandler = new MockHttpMessageHandler();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(csvStream)
        };
        mockHttpMessageHandler.SetResponse(response);

        var httpClient = new HttpClient(mockHttpMessageHandler);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        // Mock IHttpContextAccessor
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var claims = new[] { new Claim(ClaimTypes.Name, "TestUser") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = claimsPrincipal });

        // Create handler
        var postRepository = Substitute.For<IPostRepository>();
        var handler = new ImportPostsFromCsvCommandHandler(postRepository, httpClientFactory, httpContextAccessor);

        // Act
        var command = new ImportPostsFromCsvCommand(csvUrl);
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await postRepository.DidNotReceive().AddAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task Handle_ShouldHandleCsvWithInvalidHeaders()
    {
        // Arrange
        var csvUrl = "http://example.com/posts.csv";
        var csvContent = "invalid_header1,invalid_header2\n1,Content"; // Invalid header format
        var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(csvStream)
        };
        ((MockHttpMessageHandler)_mockHttpMessageHandler).SetResponse(response);

        // Act & Assert
        var command = new ImportPostsFromCsvCommand(csvUrl);
        await Assert.ThrowsAsync<ReaderException>(() => _handler.Handle(command, CancellationToken.None));

        // Verify no posts were added to the repository
        await _postRepository.DidNotReceive().AddAsync(Arg.Any<Post>());
    }
}
