using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using MythicNexus.Api.Infrastructure.Configuration;
using MythicNexus.Api.Infrastructure.Persistence;
using MythicNexus.Api.Modules.AI;
using MythicNexus.Api.Modules.Campaigns;
using MythicNexus.Api.Modules.Characters;
using MythicNexus.Api.Modules.Lore;
using MythicNexus.Api.Modules.Search;
using MythicNexus.Api.Domain.Entities;
using MythicNexus.Api.Modules.Users;

LocalEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<User>();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

builder.Services.AddDbContext<MythicNexusDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddCampaignsModule();
builder.Services.AddCharactersModule();
builder.Services.AddLoreModule();
builder.Services.AddSearchModule();
builder.Services.AddUsersModule();
builder.Services.AddAiModule();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapCampaignsEndpoints();
app.MapCharactersEndpoints();
app.MapLoreEndpoints();
app.MapSearchEndpoints();
app.MapUsersEndpoints();
app.MapAiEndpoints();

app.Run();
