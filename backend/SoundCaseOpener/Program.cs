using System.Text.Json;
using System.Text.Json.Serialization;
using SoundCaseOpener;
using SoundCaseOpener.Shared;
using SoundCaseOpener.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Options;
using NodaTime.Serialization.SystemTextJson;
using SoundCaseOpener.Core.Util;
using SoundCaseOpener.Hubs;
using SoundCaseOpener.Persistence.Util;

var builder = WebApplication.CreateBuilder(args);

bool isDev = builder.Environment.IsDevelopment();
var configurationManager = builder.Configuration;
var settings = builder.Services.LoadAndConfigureSettings(configurationManager);

builder.AddLogging();
builder.Services.AddApplicationServices(configurationManager, isDev);
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddCors(settings);
builder.Services.AddControllers(o => { o.ModelBinderProviders.Insert(0, new NodaTimeModelBinderProvider()); })
       .AddJsonOptions(o => ConfigureJsonSerialization(o, isDev));
builder.Services.ConfigureAdditionalRouteConstraints();

var app = builder.Build();

// not using HTTPS, because all production backends _have_ to be behind a reverse proxy which will handle SSL termination

app.UseCors(Setup.CorsPolicyName);
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseWebSockets();
app.MapControllers();
app.MapHub<LobbyHub>(ILobbyHub.Route).RequireCors(Setup.CorsPolicyName);
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await ImportSeedDataIfNotExists(app);

await app.RunAsync();

return;

static void ConfigureJsonSerialization(JsonOptions options, bool isDev)
{
    JsonConfig.ConfigureJsonSerialization(options.JsonSerializerOptions, isDev);
}

static async Task ImportSeedDataIfNotExists(IHost app)
{
    if (Environment.GetEnvironmentVariable("INT_TESTING") is not null)
    {
        return;
    }

    using var scope = app.Services.CreateScope();
    await using var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    IOptions<Settings> settings = scope.ServiceProvider.GetRequiredService<IOptions<Settings>>();
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    bool seedDataInserted = await Seeder.ImportSeedDataAsync(context, settings.Value);
    if (seedDataInserted)
    {
        logger.LogInformation("Initial seed data inserted");
    }
    else
    {
        logger.LogInformation("Initial seed data already present");
    }
}


// used for integration testing
public partial class Program { }