using BlogApi.DTOs;
using BlogApi.Endpoints.Internal;
using BlogApi.Models;
using BlogApi.Repository;
using BlogApi.Services;
using BlogApi.Services.Commands.Posts;
using BlogApi.Services.Queries.Posts;
using FluentValidation;
using FluentValidation.Results;
using Hangfire;
using MediatR;

namespace BlogApi.Endpoints;

public class PostEndpoints : IEndpoints
{
    private const string ContentType = "application/json";
    private const string Tag = "Posts";
    private const string BaseRoute = "posts";
    
    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("api/posts", HandleCreatePostAsync)
            .WithName("CreatePost")
            .Accepts<CreatePostDto>(ContentType)
            .Produces<Post>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapPut("/api/posts/{id:int}", HandleUpdatePostAsync)
            .WithName("UpdatePost")
            .Accepts<UpdatePostDto>(ContentType)
            .Produces<Post>(200)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapDelete("/api/posts/{id:int}",
            async (
                IMediator mediator,
                //IPostService postService, 
                int id) =>
            {
                try
                {
                   // await postService.DeletePostAsync(id);
                   await mediator.Send(new DeletePostCommand(id));
                   return Results.NoContent();
                }
                catch (Exception e)
                {
                    return Results.BadRequest(new { message = e.Message });
                }
            });

        app.MapGet("/api/posts", 
            async (
                //IPostService postService, 
                IMediator mediator,
                int pageNumber,
                int pageSize) =>
            {
                //var posts = await postService.GetPostsAsync(pageNumber, pageSize);
                var posts = await mediator.Send(new GetPostsQuery(pageNumber, pageSize));
                return Results.Ok(posts);
            });
        
        app.MapGet("/api/posts{id:int}", 
            async (
                //IPostService postService, 
                IMediator mediator,
                int id) =>
            {
                //var post = await postService.GetPostAsync(id);
                var post = await mediator.Send(new GetPostByIdQuery(id));
                return Results.Ok(post);
            });
        
        // app.MapPost("/api/posts/import", (PostImportJob job) =>
        //     {
        //         BackgroundJob.Enqueue(() => 
        //             job.ImportPostsAsync("https://fleetcor-cvp.s3.eu-central-1.amazonaws.com/blog-posts.csv"));
        //         return Results.Ok("Import job has been enqueued.");
        //     })
        //     .WithName("ImportPosts")
        //     .WithTags("Posts");
        
        app.MapPost("/api/posts/import", async (IMediator mediator, string csvUrl) =>
            {
                if (string.IsNullOrWhiteSpace(csvUrl))
                {
                    return Results.BadRequest(new { message = "CSV URL is required." });
                }

                try
                {
                    await mediator.Send(new ImportPostsFromCsvCommand(csvUrl));
                    return Results.Ok("Import job completed successfully.");
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .WithName("ImportPosts")
            .WithTags("Posts");

        
    }
    
      
    private static async Task<IResult> HandleCreatePostAsync(
        //IPostService postService,
        IMediator mediator,
        CreatePostDto createPostDto,
        IValidator<CreatePostDto> validator)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(createPostDto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
            //var post = await postService.CreatePostAsync(createPostDto);
            var post = await mediator.Send(new CreatePostCommand(createPostDto));
            return Results.Created($"api/posts/{post.Id}", post);
        }
        catch (Exception e)
        {
            return Results.BadRequest(new { message = e.Message });
        }
    }
    
    private static async Task<IResult> HandleUpdatePostAsync(
        //IPostService postService, 
        IMediator mediator,
        int id, 
        UpdatePostDto updatePostDto,
        IValidator<UpdatePostDto> validator)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(updatePostDto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
            //var post = await postService.UpdatePostAsync(id, updatePostDto);
            var post = await mediator.Send(new UpdatePostCommand(id, updatePostDto));
            return Results.Ok(post);
        }
        catch (Exception e)
        {
            return Results.BadRequest(new { message = e.Message });
        }
    }
    
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(typeof(Program)); // Register MediatR
       // services.AddScoped<IPostService, PostService>();
        //services.AddScoped<IPostRepository, PostRepository>();
    }
}