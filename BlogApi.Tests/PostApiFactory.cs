
using BlogApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace BlogApi.Tests;

public class PostApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .WithPortBinding(5555, 5432)
        .Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IHostedService));
            services.RemoveAll(typeof(AppDbContext));
            services.AddDbContext<AppDbContext>(
                optionsBuilder => optionsBuilder.UseNpgsql(
                    "Server=localhost;Port=5555;Database=mydb;User ID=course;Password=changeme;"));
            // Inject a mock mediator inside the factory
            var mediatorMock = Substitute.For<IMediator>();
            services.AddSingleton(mediatorMock);
        });

        builder.ConfigureServices(services =>
        {
            // Add a test authentication handler
            services.AddAuthentication("TestAuth")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });
        });

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["JwtSettings:SecretKey"] = "ThisIsASecretKeyForJwt1234567890",
                ["JwtSettings:Issuer"] = "BlogApi",
                ["JwtSettings:Audience"] = "BlogApiAudience"
            });
        });
    }

    public async Task InitializeAsync()
    {
        // Start the Testcontainer and wait for readiness
        await _dbContainer.StartAsync();
        await WaitForDatabaseReadiness();
    }

    private async Task WaitForDatabaseReadiness()
    {
        using var connection = new NpgsqlConnection(
            $"Server=localhost;Port={_dbContainer.GetMappedPublicPort(5432)};Database=testdb;User ID=testuser;Password=testpass;");
        
        for (int i = 0; i < 10; i++) // Retry up to 10 times
        {
            try
            {
                await connection.OpenAsync();
                return; // Success
            }
            catch
            {
                await Task.Delay(1000); // Wait 1 second before retrying
            }
        }
        throw new Exception("Database is not ready after 10 attempts");
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Post> Posts { get; set; }
    public DbSet<User> Users { get; set; } 
}
