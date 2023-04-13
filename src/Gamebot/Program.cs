using EntityFramework.Exceptions.Sqlite;
using Gamebot;
using Gamebot.Persistence;
using Gamebot.Routes;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using TwitchLib.Api;
using TwitchLib.Client;

Log.Logger = new LoggerConfiguration().MinimumLevel
    .Debug()
    //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
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
    options =>
    {
        options.UseSqlite("Data Source=game.db");
        options.UseExceptionProcessor();
    }
);
TwitchClient twitchClient = Bot.CreateTwitchClient();
builder.Services.AddSingleton(twitchClient);

TwitchAPI twitchApi = new TwitchAPI
{
    Settings = { ClientId = options.ClientId, Secret = options.ClientSecret }
};
builder.Services.AddSingleton(twitchApi);

builder.Services.AddFusionCache();
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

app.MapGroup("/").MapAuthRedirect();

/*
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BotDbContext>();
    db.Database.Migrate();
}
*/

await app.RunAsync();
