using System.Net;
using System.Text;
using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services.Commands.Posts;
using BlogApi.Services.Helpers;
using NSubstitute;

namespace BlogApi.Tests.Services.Commands.Posts
{
    public class ImportPostsFromCsvCommandHandlerTests
    {
        private readonly IPostRepository _postRepository;
        private readonly HttpClient _httpClient;
        private readonly ImportPostsFromCsvCommandHandler _handler;
        private readonly MockHttpMessageHandler _mockHttpMessageHandler;

        public ImportPostsFromCsvCommandHandlerTests()
        {
            _postRepository = Substitute.For<IPostRepository>();

            // Create the mock handler and assign it to HttpClient
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_mockHttpMessageHandler);

            _handler = new ImportPostsFromCsvCommandHandler(_postRepository, _httpClient);
        }

        [Fact]
        public async Task Handle_ShouldImportPostsFromCsv()
        {
            // Arrange
            var csvUrl = "http://example.com/posts.csv";
            var csvContent = "Title,Content\nPost 1,Content 1\nPost 2,Content 2";
            var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            // Mock HttpClient to return the CSV stream
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(csvStream)
            };

            // Set the response in the mock handler
            _mockHttpMessageHandler.SetResponse(httpResponseMessage);

            // Act
            var command = new ImportPostsFromCsvCommand(csvUrl);
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            // Verify that the repository's AddAsync method was called for each post
            await _postRepository.Received(2).AddAsync(Arg.Any<Post>());

            // Verify specific posts were added (optional)
            await _postRepository.Received(1).AddAsync(Arg.Is<Post>(p => p.Title == "Post 1" && p.Content == "Content 1"));
            await _postRepository.Received(1).AddAsync(Arg.Is<Post>(p => p.Title == "Post 2" && p.Content == "Content 2"));
        }

        [Fact]
        public async Task Handle_ShouldHandleEmptyCsv()
        {
            // Arrange
            var csvUrl = "http://example.com/empty.csv";
            var csvContent = "Title,Content\n";
            var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(csvStream)
            };

            // Set the response in the mock handler
            _mockHttpMessageHandler.SetResponse(httpResponseMessage);

            // Act
            var command = new ImportPostsFromCsvCommand(csvUrl);
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            // Verify that no posts were added
            await _postRepository.DidNotReceive().AddAsync(Arg.Any<Post>());
        }

        [Fact]
        public async Task Handle_ShouldHandleHttpClientError()
        {
            // Arrange
            var csvUrl = "http://example.com/error.csv";

            // Set an exception in the mock handler
            _mockHttpMessageHandler.SetException(new HttpRequestException("Failed to fetch CSV"));

            // Act & Assert
            var command = new ImportPostsFromCsvCommand(csvUrl);
            await Assert.ThrowsAsync<HttpRequestException>(() => _handler.Handle(command, CancellationToken.None));

            // Verify that no posts were added
            await _postRepository.DidNotReceive().AddAsync(Arg.Any<Post>());
        }

       
    }
}