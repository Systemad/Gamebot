using System.Collections.Concurrent;
using Gamebot.Helper;
using Gamebot.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace Gamebot;

public class TwitchClientWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<BotDbContext> _contextFactory;
    private TwitchClient TwitchClient;
    private readonly API _api;
    private ConcurrentDictionary<string, string> CacheKeys { get; set; }

    public TwitchClientWorkerService(
        IHostApplicationLifetime appLifetime,
        IServiceProvider serviceProvider,
        IDbContextFactory<BotDbContext> contextFactory,
        API api
    )
    {
        _serviceProvider = serviceProvider;
        _contextFactory = contextFactory;
        _api = api;
        //appLifetime.ApplicationStarted.Register(OnStarted);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TwitchClient = _serviceProvider.GetRequiredService<TwitchClient>();
        TwitchClient.Connect();
        Log.Information(
            "ExecuteAsync: Starting - Twitchbot status {@Status}",
            TwitchClient.IsConnected
        );

        await using var context = _contextFactory.CreateDbContext();
        var channels = await context.Channels.ToListAsync();

        foreach (var channel in channels)
        {
            Log.Information($"Connecting to {channel.Name}");
            TwitchClient.JoinChannel(channel.Name);
        }
    }

    public override void Dispose()
    {
        TwitchClient.Disconnect();
        Log.Information("Dispose stopping - Twitchbot status {@Status}", TwitchClient.IsConnected);
    }

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
}
