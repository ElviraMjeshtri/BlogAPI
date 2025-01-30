using BlogApi.DTOs.Auth;
using BlogApi.Endpoints.Internal;
using BlogApi.Services.Commands.Auth;
using MediatR;

namespace BlogApi.Endpoints;

public class AuthEndpoints : IEndpoints
{
    private const string Tag = "Auth";
    private const string Login = "Login";
    private const string RegisterUser = "RegisterUser";
    private const string LogoutUser = "LogoutUser";
    private const string LoginEndpoint = "api/auth/login";
    private const string LogoutEndpoint = "/api/auth/logout";
    private const string RegisterEndpoint = "/api/auth/register";
    
    
    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
       
        app.MapPost(LoginEndpoint, HandleLoginPostAsync)
            .WithName(Login)
            .WithTags(Tag);

        app.MapPost(RegisterEndpoint, async (IMediator mediator, RegisterUserDto registerDto) =>
            {
                var response = await mediator.Send(new RegisterUserCommand(registerDto));
                return Results.Ok(response);
            })
            .WithName(RegisterUser)
            .WithTags(Tag);
        ;

        app.MapPost(LogoutEndpoint, async (IMediator mediator, string token) =>
            {
                await mediator.Send(new LogoutUserCommand(token));
                return Results.Ok("Logged out successfully.");
            })
            .WithName(LogoutUser)
            .WithTags(Tag);
        ;
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