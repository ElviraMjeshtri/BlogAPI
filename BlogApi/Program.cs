using System.Text;
using BlogApi.Authentication;
using BlogApi.data;
using BlogApi.DTOs.Validators;
using BlogApi.Endpoints.Internal;
using FluentValidation;
using BlogApi.Repository;
using BlogApi.Services;
using BlogApi.Services.Commands.Auth;
using BlogApi.Services.Commands.Posts;
using BlogApi.Services.Queries.Posts;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.MemoryStorage;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

// Configure JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<TokenServices>();

// Configure authentication

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };

    });
//configure authorization policies

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin")); // Matches "role": "admin"
    options.AddPolicy("UserOnly", policy => policy.RequireRole("user"));   // Matches "role": "user"
});



// Register MediatR
builder.Services.AddMediatR(typeof(RegisterUserCommandHandler).Assembly);
builder.Services.AddMediatR(typeof(LoginUserCommandHandler).Assembly);
builder.Services.AddMediatR(typeof(CreatePostCommandHandler).Assembly);
builder.Services.AddMediatR(typeof(UpdatePostCommandHandler).Assembly);
builder.Services.AddMediatR(typeof(DeletePostCommandHandler).Assembly);
builder.Services.AddMediatR(typeof(GetPostByIdQueryHandler).Assembly);
builder.Services.AddMediatR(typeof(GetPostsQueryHandler).Assembly);
builder.Services.AddMediatR(typeof(ImportPostsFromCsvCommandHandler).Assembly);


builder.Services.AddValidatorsFromAssemblyContaining<CreatePostDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Register IPostRepository with PostRepository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ITokenBlacklistRepository, DatabaseTokenBlacklistRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<PostImportJob>();
builder.Services.AddScoped<AuthService>();


// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});



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

// Enable authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

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

