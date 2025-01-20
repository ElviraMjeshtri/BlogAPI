using BlogApi.data;
using BlogApi.DTOs.Validators;
using BlogApi.Endpoints.Internal;
using FluentValidation;
using BlogApi.Repository;
using BlogApi.Services;
using BlogApi.Services.Commands.Posts;
using BlogApi.Services.Queries.Posts;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.MemoryStorage;
using MediatR;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Register MediatR
builder.Services.AddMediatR(typeof(CreatePostCommandHandler).Assembly);
builder.Services.AddMediatR(typeof(UpdatePostCommandHandler).Assembly);
builder.Services.AddMediatR(typeof(DeletePostCommandHandler).Assembly);
builder.Services.AddMediatR(typeof(GetPostByIdQueryHandler).Assembly);
builder.Services.AddMediatR(typeof(GetPostsQueryHandler).Assembly);
builder.Services.AddMediatR(typeof(ImportPostsFromCsvCommandHandler).Assembly);


builder.Services.AddValidatorsFromAssemblyContaining<CreatePostDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Register IPostRepository with PostRepository
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<PostImportJob>();


// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<PostImportJob>();


// Configure Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage(); // Use memory storage for simplicity
});

builder.Services.AddHangfireServer();
// Register custom endpoints
builder.Services.AddEndpoints<Program>(builder.Configuration);
builder.Services.AddMediatR(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Apply migrations automatically
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
//     dbContext.Database.Migrate();
// }

// Register endpoints from the separate class
app.UseEndpoints<Program>();

// Add Hangfire Dashboard (optional)
app.UseHangfireDashboard();

app.Run();

public partial class Program { }

