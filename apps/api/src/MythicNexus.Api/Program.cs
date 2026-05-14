using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MythicNexus.Api.Infrastructure.Exceptions;
using MythicNexus.Api.Modules.AI;
using MythicNexus.Api.Modules.Campaigns;
using MythicNexus.Api.Modules.Characters;
using MythicNexus.Api.Modules.Lore;
using MythicNexus.Api.Modules.Search;
using MythicNexus.Api.Infrastructure.Email;
using MythicNexus.Api.Modules.Tenants;
using MythicNexus.Api.Modules.Users.Endpoints;
using MythicNexus.Application.DependencyInjection;
using MythicNexus.Application.Users;
using MythicNexus.Application.Users.Contracts;
using MythicNexus.Application.Validation;
using MythicNexus.Infrastructure.Configuration;
using MythicNexus.Infrastructure.DependencyInjection;
using MythicNexus.Infrastructure.Middleware;
using System.Security.Claims;
using System.Threading.RateLimiting;

LocalEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    static string PartitionIp(HttpContext ctx) =>
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    options.AddPolicy(
        "auth_login",
        context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: PartitionIp(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));

    options.AddPolicy(
        "auth_register",
        context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: PartitionIp(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));

    options.AddPolicy(
        "auth_recovery",
        context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: PartitionIp(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(15),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));

    options.AddPolicy(
        "auth_verify",
        context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: PartitionIp(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            }));
});

builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p =>
        p.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddInfrastructurePersistence(connectionString);
builder.Services.AddApplication(builder.Configuration);

builder.Services.Configure<EmailOutboxOptions>(builder.Configuration.GetSection(EmailOutboxOptions.SectionName));
builder.Services.AddSingleton<IEmailOutbox, LocalFileEmailOutbox>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
        var secretKey = jwtSection["SecretKey"] ?? string.Empty;
        var issuer = jwtSection["Issuer"] ?? string.Empty;
        var audience = jwtSection["Audience"] ?? string.Empty;

        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.NameIdentifier,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCampaignsModule();
builder.Services.AddCharactersModule();
builder.Services.AddLoreModule();
builder.Services.AddSearchModule();
builder.Services.AddAiModule();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Supabase"))
{
    app.MapOpenApi();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapCampaignsEndpoints();
app.MapCharactersEndpoints();
app.MapLoreEndpoints();
app.MapSearchEndpoints();
app.MapUsersEndpoints();
app.MapTenantsEndpoints();
app.MapAiEndpoints();

app.Run();
