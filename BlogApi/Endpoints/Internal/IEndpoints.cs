namespace BlogApi.Endpoints.Internal;

public interface IEndpoints
{
    public static abstract void DefineEndpoints(IEndpointRouteBuilder app);
    public static abstract void AddServices(IServiceCollection app, IConfiguration configuration);
}