using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services.Queries.Posts;
using NSubstitute;

namespace BlogApi.Tests.Posts;

 public class GetPostsQueryHandlerTests
    {
        private readonly IPostRepository _repository;
        private readonly GetPostsQueryHandler _handler;

        public GetPostsQueryHandlerTests()
        {
            _repository = Substitute.For<IPostRepository>();
            _handler = new GetPostsQueryHandler(_repository);
        }

        [Fact]
        public async Task Handle_ShouldReturnPaginatedPostDtos_WhenPostsExist()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = "Post 1",
                    Content = "Content 1",
                    FriendlyUrl = "post-1",
                    DateCreated = DateTime.UtcNow,
                    CreatedBy = "user1"
                },
                new Post
                {
                    Id = 2,
                    Title = "Post 2",
                    Content = "Content 2",
                    FriendlyUrl = "post-2",
                    DateCreated = DateTime.UtcNow,
                    CreatedBy = "user2"
                }
            };

            _repository.GetPaginatedAsync(pageNumber, pageSize).Returns(posts);

            var query = new GetPostsQuery(pageNumber, pageSize);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            var firstPost = result.First();
            Assert.Equal(posts[0].Id, firstPost.Id);
            Assert.Equal(posts[0].Title, firstPost.Title);
            Assert.Equal(posts[0].Content, firstPost.Content);
            Assert.Equal(posts[0].FriendlyUrl, firstPost.FriendlyUrl);
            Assert.Equal(posts[0].DateCreated, firstPost.DateCreated);
            Assert.Equal(posts[0].CreatedBy, firstPost.CreatedBy);

            await _repository.Received(1).GetPaginatedAsync(pageNumber, pageSize);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoPostsExist()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var posts = new List<Post>();

            _repository.GetPaginatedAsync(pageNumber, pageSize).Returns(posts);

            var query = new GetPostsQuery(pageNumber, pageSize);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            await _repository.Received(1).GetPaginatedAsync(pageNumber, pageSize);
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentNullException_WhenQueryIsNull()
        {
            // Arrange
            GetPostsQuery query = null;

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() =>
                _handler.Handle(query, CancellationToken.None)
            );
        }
    }