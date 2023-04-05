using Microsoft.Extensions.Configuration;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Enums;
using TwitchLib.Communication.Models;

namespace Gamebot;

public static class Bot
{
    public static TwitchClient CreateTwitchClient()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var options = new TwitchOptions();
        config.GetSection(TwitchOptions.Twitch).Bind(options);

        ConnectionCredentials credentials = new ConnectionCredentials(
            options.Username,
            options.AccessToken
        );
        var clientOptions = new ClientOptions
        {
            //ClientType = ClientType.Chat,
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        WebSocketClient customClient = new WebSocketClient(clientOptions);
        TwitchClient client = new TwitchClient(customClient);
        client.Initialize(credentials, "LossyXP");
        return client;
    }
}
