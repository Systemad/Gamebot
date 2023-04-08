using Gamebot;
using Gamebot.Persistence;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using Serilog.Events;
using TwitchLib.Api;

Log.Logger = new LoggerConfiguration().MinimumLevel
    .Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("log/log_.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddSingleton<ChannelService>();
builder.Services.AddFusionCache();
builder.Services.AddHostedService<TwitchClientWorkerService>().AddSingleton<API>();

// TODO: Wait for this https://github.com/serilog/serilog-extensions-hosting/pull/70
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

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var options = new TwitchOptions();
config.GetSection(TwitchOptions.Twitch).Bind(options);

// Figure out join channel directly from here! global TwitchClient or eventhandler something
app.MapGet(
    "/redirect/",
    async (HttpRequest request, BotDbContext dbContext) =>
    {
        var queryString = request.QueryString;
        var queryDictionary = QueryHelpers.ParseQuery(queryString.Value);
        var context = request.HttpContext;
        if (queryDictionary.TryGetValue("code", out var authCode))
        {
            var twitchApi = new TwitchAPI
            {
                Settings = { ClientId = options.ClientId, Secret = options.ClientSecret }
            };

            var authTokenResponse = await twitchApi.Auth.GetAccessTokenFromCodeAsync(
                authCode,
                options.ClientSecret,
                options.RedirectUri
            );

            var user = await twitchApi.Helix.Users.GetUsersAsync(
                accessToken: authTokenResponse.AccessToken
            );

            var channel = new TwitchChannel
            {
                Id = user.Users.First().Id,
                Name = user.Users.First().Login,
                Added = DateTimeOffset.Now
            };
            dbContext.Channels.Add(channel);
            await dbContext.SaveChangesAsync();
        }
        context.Response.Redirect("/");
    }
);
await app.RunAsync();
