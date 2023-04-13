using System.Collections.Concurrent;
using Gamebot.Helper;
using Gamebot.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Gamebot;

public class TwitchBotClient
{
    private readonly IDbContextFactory<BotDbContext> _contextFactory;
    private readonly API _api;
    private readonly TwitchClient TwitchClient;
    private ConcurrentDictionary<string, string> CacheKeys { get; set; }

    public TwitchBotClient(API api, IDbContextFactory<BotDbContext> contextFactory)
    {
        CacheKeys = new();
        _api = api;
        _contextFactory = contextFactory;
        TwitchClient = Bot.CreateTwitchClient();
        TwitchClient.OnConnected += async (sender, args) => await OnConnectedAsync(sender, args);
    }

    public async Task JoinChannelAsync(TwitchChannel channel)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.Channels.Add(channel);
        await context.SaveChangesAsync();
        TwitchClient.JoinChannel(channel.Name);
    }

    public void ConnectBot()
    {
        TwitchClient.Connect();
        Log.Information(BotStatus.ToString());
    }

    public void DisconnectBot()
    {
        TwitchClient.Disconnect();
        Log.Information(BotStatus.ToString());
    }

    public bool BotStatus => TwitchClient.IsConnected;

    private async Task OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
    {
        Log.Information(e.Command.ChatMessage.Message);

        // TODO: FIX
        if (!e.Command.ChatMessage.IsBroadcaster || !e.Command.ChatMessage.IsModerator)
            return;

        Log.Information("after ismod isbroadcaster");

        if (e.Command.CommandIdentifier is not '!')
            return;

        Log.Information("commandidentifier: ");

        if (e.Command.CommandText.Equals("match"))
        {
            if (CacheKeys.TryGetValue(e.Command.ChatMessage.Channel, out string cachekey))
            {
                var match = await _api.GetMatch(cachekey);
                var matchString = CommandHelper.GetCommandString(match);
                TwitchClient.SendMessage(e.Command.ChatMessage.Channel, matchString);
            }
        }

        if (e.Command.CommandText.Equals("setmatch"))
        {
            if (string.IsNullOrWhiteSpace(e.Command.ArgumentsAsString))
                return;

            var matchLink = await _api.GetMatchLink(e.Command.ArgumentsAsString);

            if (string.IsNullOrEmpty(matchLink))
                TwitchClient.SendMessage(
                    e.Command.ChatMessage.Channel,
                    "Match not found, please try inserting full name!"
                );

            if (CacheKeys.TryAdd(e.Command.ChatMessage.Channel, matchLink))
            {
                var match = await _api.GetMatch(matchLink);
                var matchString = CommandHelper.GetCommandString(match);
                TwitchClient.SendMessage(e.Command.ChatMessage.Channel, matchString);
            }
        }
    }

    private async Task OnConnectedAsync(object sender, OnConnectedArgs args)
    {
        Log.Information($"OnConnectedAsync: Connected to {args.BotUsername}");
        await using var context = _contextFactory.CreateDbContext();
        var channels = await context.Channels.ToListAsync();

        foreach (var channel in channels)
        {
            Log.Information($"Connecting to {channel.Name}");
            TwitchClient.JoinChannel(channel.Name);
        }
    }
}

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
