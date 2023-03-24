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
        TwitchClient client = new();
        ConnectionCredentials credentials = new ConnectionCredentials("LossyXP", "access_token");
        var clientOptions = new ClientOptions
        {
            ClientType = ClientType.Chat,
            //MessagesAllowedInPeriod = 750,
            //ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        WebSocketClient customClient = new WebSocketClient(clientOptions);
        client = new TwitchClient(customClient);
        client.Initialize(credentials, "channel");

        //client.OnMessageReceived += OnMessageReceived;
        return client;
    }
}
