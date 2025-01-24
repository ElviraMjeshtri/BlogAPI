using BlogApi.DTOs;
using BlogApi.Endpoints.Internal;
using BlogApi.Models;
using BlogApi.Services.Commands.Posts;
using BlogApi.Services.Queries.Posts;
using FluentValidation;
using FluentValidation.Results;
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
            .RequireAuthorization("AdminOnly")
            .WithName("CreatePost")
            .Accepts<CreatePostDto>(ContentType)
            .Produces<Post>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapPut("/api/posts/{id:int}", HandleUpdatePostAsync)
            .RequireAuthorization("AdminOnly")
            .WithName("UpdatePost")
            .Accepts<UpdatePostDto>(ContentType)
            .Produces<Post>(200)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapDelete("/api/posts/{id:int}", HandleDeletePostAsync)
            .RequireAuthorization("AdminOnly")
            .WithTags(Tag);

        app.MapGet("/api/posts", HandleGetPostsAsync)
            .RequireAuthorization("UserOnly")
            .WithTags(Tag);

        app.MapGet("/api/posts/{id:int}", HandleGetByIdPostAsync)
            .RequireAuthorization()
            .WithTags(Tag);


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
            .RequireAuthorization("AdminOnly")
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

    private static async Task<IResult> HandleDeletePostAsync(
        IMediator mediator,
        int id)
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
    }

    private static async Task<IResult> HandleGetPostsAsync(
        IMediator mediator,
        int pageNumber,
        int pageSize)
    {
        var posts = await mediator.Send(new GetPostsQuery(pageNumber, pageSize));
        return Results.Ok(posts);
    }

    private static async Task<IResult> HandleGetByIdPostAsync( IMediator mediator,
        int id)
    {
        var post = await mediator.Send(new GetPostByIdQuery(id));
        return Results.Ok(post);
    }

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(typeof(Program)); 
    }
}