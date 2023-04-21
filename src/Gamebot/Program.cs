using EntityFramework.Exceptions.Sqlite;
using Gamebot;
using Gamebot.Persistence;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

Log.Logger = new LoggerConfiguration().MinimumLevel
    .Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    //.WriteTo.File("log/log_.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var options = new TwitchOptions();
config.GetSection(TwitchOptions.Twitch).Bind(options);

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration
        //.ReadFrom.Configuration(context.Configuration)
        .ReadFrom
            .Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

builder.Services.AddDbContextFactory<BotDbContext>(
    opts => opts.UseSqlite("Data Source=game.db").UseExceptionProcessor()
);

TwitchAPI twitchApi = new TwitchAPI
{
    Settings = { ClientId = options.ClientId, Secret = options.ClientSecret }
};
ConnectionCredentials credentials = new ConnectionCredentials(
    options.Username,
    options.AccessToken
);

var clientOptions = new ClientOptions
{
    //ClientType = ClientType.Chat,
    MessagesAllowedInPeriod = 2000,
    ThrottlingPeriod = TimeSpan.FromSeconds(30)
};
WebSocketClient customClient = new WebSocketClient(clientOptions);
TwitchClient twitchClient = new TwitchClient(customClient);
twitchClient.Initialize(credentials, options.Username);

builder.Services.AddSingleton(twitchApi);
builder.Services.AddSingleton(twitchClient);

builder.Services.AddFusionCache();
builder.Services.AddSingleton<ContentParser>();
builder.Services.AddSingleton<API>();

builder.Services.AddHostedService<TwitchClientWorkerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

/*
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BotDbContext>();
    db.Database.Migrate();
}
*/

await app.RunAsync();
