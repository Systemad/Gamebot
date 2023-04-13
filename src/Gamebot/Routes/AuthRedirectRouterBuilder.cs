using EntityFramework.Exceptions.Common;
using Gamebot.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using TwitchLib.Api;
using TwitchLib.Client;

namespace Gamebot.Routes;

public static class AuthRedirectRouterBuilder
{
    /*    TODO: Potentially skip this?
     *     Instead just redirect to index:8080/urlquerytoken
     *     and do everything here but inside index.razor
     *      then somehow die account id in cookies? signedIn: true : accountObject Info
     *      also create new serviceclass that inject TwitchClient
     *      and add functions like connecttochannel, disocnnect // To that inside razor
     */
    public static RouteGroupBuilder MapAuthRedirect(this RouteGroupBuilder routeBuilder)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var options = new TwitchOptions();
        config.GetSection(TwitchOptions.Twitch).Bind(options);

        routeBuilder.MapGet(
            "/redirect/",
            async (
                HttpRequest request,
                [FromServices] TwitchClient twitchBotClient,
                [FromServices] TwitchAPI twitchApi,
                [FromServices] IDbContextFactory<BotDbContext> contextFactory
            ) =>
            {
                var queryString = request.QueryString;
                var queryDictionary = QueryHelpers.ParseQuery(queryString.Value);
                var context = request.HttpContext;
                if (queryDictionary.TryGetValue("code", out var authCode))
                {
                    /*
                    var twitchApi = new TwitchAPI
                    {
                        Settings = { ClientId = options.ClientId, Secret = options.ClientSecret }
                    };
                    */
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

                    twitchBotClient.JoinChannel(channel.Name);
                    await using var dbcontext = contextFactory.CreateDbContext();
                    dbcontext.Channels.Add(channel);

                    try
                    {
                        await dbcontext.SaveChangesAsync();
                    }
                    catch (UniqueConstraintException e)
                    {
                        Console.WriteLine("already added");
                        throw;
                    }
                }
                //context.Response.Redirect("/");
            }
        );
        return routeBuilder;
    }
}
