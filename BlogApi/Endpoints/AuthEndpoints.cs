using BlogApi.DTOs.Auth;
using BlogApi.Endpoints.Internal;
using BlogApi.Services.Commands.Auth;
using MediatR;

namespace BlogApi.Endpoints;

public class AuthEndpoints : IEndpoints
{
    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/login", HandleLoginPostAsync)
            .WithName("Login")
            .WithTags("Auth");
        
        app.MapPost("/api/auth/register", async (IMediator mediator, RegisterUserDto registerDto) =>
            {
                var response = await mediator.Send(new RegisterUserCommand(registerDto));
                return Results.Ok(response);
            })
            .WithName("RegisterUser")
            .WithTags("Auth");;
        
        app.MapPost("/api/auth/logout", async (IMediator mediator, string token) =>
            {
                await mediator.Send(new LogoutUserCommand(token));
                return Results.Ok("Logged out successfully.");
            })
            .WithName("LogoutUser")
            .WithTags("Auth");;
    }

    private static async Task<IResult> HandleLoginPostAsync(IMediator mediator,
        LoginRequestDto request)
    {
        try
        {
            var result = await mediator.Send(new LoginUserCommand(request));
            return Results.Ok(result);

        }
        catch (Exception e)
        {
            return Results.BadRequest(new { message = e.Message });
        }
    }

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(typeof(Program)); // Register MediatR
    }
}
