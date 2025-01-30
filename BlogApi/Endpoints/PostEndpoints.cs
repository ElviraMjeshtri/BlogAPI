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
    private const string CreatePostEndpointUrl = "api/posts";
    private const string CreatePostEndpointName = "CreatePost";
    private const string UpdatePostEndpointUrl = "/api/posts/{id:int}";
    private const string UpdatePostEndpointName = "UpdatePost";
    private const string DeletePostEndpointUrl = "/api/posts/{id:int}";
    private const string DeletePostEndpointName = "DeletePost";
    private const string GetPostsEndpointName = "GetPosts";
    private const string GetPostsEndpointUrl = "/api/posts";
    private const string GetPostEndpointUrl = "/api/posts/{id:int}";
    private const string GetPostEndpointName = "GetPost";
    private const string AdminOnly = "AdminOnly";
    private const string ImportPostsEndpointName = "ImportPosts";
    private const string ImportPostsEndpointUrl = "/api/posts/import";

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(CreatePostEndpointUrl, HandleCreatePostAsync)
            .RequireAuthorization(AdminOnly)
            .WithName(CreatePostEndpointName)
            .Accepts<CreatePostDto>(ContentType)
            .Produces<Post>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapPut(UpdatePostEndpointUrl, HandleUpdatePostAsync)
            .RequireAuthorization(AdminOnly)
            .WithName(UpdatePostEndpointName)
            .Accepts<UpdatePostDto>(ContentType)
            .Produces<Post>(200)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);

        app.MapDelete(DeletePostEndpointUrl, HandleDeletePostAsync)
            .RequireAuthorization(AdminOnly)
            .WithName(DeletePostEndpointName)
            .WithTags(Tag);

        app.MapGet(GetPostsEndpointUrl, HandleGetPostsAsync)
            .RequireAuthorization()
            .WithName(GetPostsEndpointName)
            .WithTags(Tag);

        app.MapGet(GetPostEndpointUrl, HandleGetByIdPostAsync)
            .RequireAuthorization()
            .WithName(GetPostEndpointName)
            .WithTags(Tag);


        app.MapPost(ImportPostsEndpointUrl, async (IMediator mediator, string csvUrl) =>
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
            .RequireAuthorization(AdminOnly)
            .WithName(ImportPostsEndpointName)
            .WithTags(Tag);
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
    
    // private static async Task<IResult> HandleDeletePostAsync(
    //     IMediator mediator,
    //     int id)
    // {
    //     try
    //     {
    //         // await postService.DeletePostAsync(id);
    //         await mediator.Send(new DeletePostCommand(id));
    //         return Results.NoContent();
    //     }
    //     catch (Exception e)
    //     {
    //         return Results.BadRequest(new { message = e.Message });
    //     }
    // }
    //
    private static async Task<IResult> HandleDeletePostAsync(IMediator mediator, int id)
    {
        var result = await mediator.Send(new DeletePostCommand(id));

        if (!result.IsSuccess)
        {
            return Results.StatusCode((int)result.StatusCode); // Returns 404 if post not found
        }

        return Results.NoContent(); // 204 No Content on successful deletion
    }


    private static async Task<IResult> HandleGetPostsAsync(
        IMediator mediator,
        int pageNumber,
        int pageSize)
    {
        var posts = await mediator.Send(new GetPostsQuery(pageNumber, pageSize));
        return Results.Ok(posts);
    }

    private static async Task<IResult> HandleGetByIdPostAsync(IMediator mediator,
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