using System.Collections.Concurrent;
using Gamebot.Helper;
using Gamebot.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Models.Responses.Messages.AutomodCaughtMessage;

namespace Gamebot;

public class TwitchClientWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<BotDbContext> _contextFactory;
    private TwitchClient _twitchClient;

    private API _api;
    private readonly ILogger<TwitchClientWorkerService> _logger;
    private ConcurrentDictionary<string, string> CacheKeys { get; set; }

    public TwitchClientWorkerService(
        //IHostApplicationLifetime appLifetime,
        IServiceProvider serviceProvider,
        IDbContextFactory<BotDbContext> contextFactory,
        ILogger<TwitchClientWorkerService> logger
    )
    {
        CacheKeys = new ConcurrentDictionary<string, string>();
        _serviceProvider = serviceProvider;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _api = _serviceProvider.GetRequiredService<API>();
            _twitchClient = _serviceProvider.GetRequiredService<TwitchClient>();
            _twitchClient.OnJoinedChannel += OnJoinedChannel;
            _twitchClient.OnLeftChannel += OnLeftChannel;
            _twitchClient.OnLog += OnLog;
            _twitchClient.OnMessageReceived += OnMessageReceived;
            _twitchClient.OnIncorrectLogin += OnIncorrectLogin;
            _twitchClient.OnChatCommandReceived += async (sender, args) =>
                await OnChatCommandReceived(sender, args);
            _twitchClient.OnConnected += async (sender, args) =>
                await OnConnectedAsync(sender, args);
            _twitchClient.Connect();
            Log.Information(
                "ExecuteAsync: Starting - Twitchbot status {@Status}",
                _twitchClient.IsConnected
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Dispose();
        }

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _twitchClient.Disconnect();
        Log.Information("Dispose stopping - Twitchbot status {@Status}", _twitchClient.IsConnected);
        base.Dispose();
    }

    private void OnMessageReceived(object sender, OnMessageReceivedArgs args) =>
        Log.Information(
            "TwitchClient {@channel}: {@message}",
            args.ChatMessage.Channel,
            args.ChatMessage.Message
        );

    private void OnJoinedChannel(object sender, OnJoinedChannelArgs args) =>
        Log.Information("TwitchClient: Joined channel {channel}", args.Channel);

    private void OnLeftChannel(object sender, OnLeftChannelArgs args) =>
        Log.Information("TwitchClient: Left channel {channel}", args.Channel);

    private async Task OnConnectedAsync(object sender, OnConnectedArgs args)
    {
        Log.Information("OnConnectedAsync: Connected to {@username}", args.BotUsername);
        await using var context = _contextFactory.CreateDbContext();
        var channels = await context.Channels.ToListAsync();

        foreach (var channel in channels)
        {
            Log.Information("Connecting to {@channel}", channel.Name);
            _twitchClient.JoinChannel(channel.Name);
        }
    }

    private async Task OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
    {
        // TODO: FIX
        var mod = e.Command.ChatMessage.IsModerator;
        var broadcaster = e.Command.ChatMessage.IsBroadcaster;

        //if (mod == false)
        //    return;

        if (broadcaster == false)
            return;

        //if (e.Command.CommandIdentifier is not '!')
        //    return;

        if (e.Command.CommandText.Equals("match"))
        {
            if (CacheKeys.TryGetValue(e.Command.ChatMessage.Channel, out string cachekey))
            {
                var match = await _api.GetMatch(cachekey);
                var matchString = CommandHelper.GetCommandString(match);
                _twitchClient.SendMessage(e.Command.ChatMessage.Channel, matchString);
            }
            else
            {
                _twitchClient.SendMessage(e.Command.ChatMessage.Channel, "No match set!");
            }
        }

        if (e.Command.CommandText.Equals("setmatch"))
        {
            var teamName = e.Command.ArgumentsAsList.First();
            var teamNameString = e.Command.ArgumentsAsString;
            if (string.IsNullOrWhiteSpace(teamNameString))
                return;

            var matchLink = await _api.GetMatchLink(teamNameString);

            if (string.IsNullOrEmpty(matchLink))
                _twitchClient.SendMessage(
                    e.Command.ChatMessage.Channel,
                    "Match not found, please try inserting full name!"
                );

            if (CacheKeys.TryAdd(e.Command.ChatMessage.Channel, matchLink))
            {
                var match = await _api.GetMatch(matchLink);
                var matchString = CommandHelper.GetCommandString(match);
                _twitchClient.SendMessage(e.Command.ChatMessage.Channel, matchString);
            }
        }
    }

    private void OnLog(object sender, OnLogArgs e) =>
        Log.Information("{@date}:{@username} - {@data}", e.DateTime, e.BotUsername, e.Data);

    private void OnIncorrectLogin(object sender, OnIncorrectLoginArgs args) =>
        Log.Information(
            "TwitchClient: OnIncorrectLogin channel {@message}",
            args.Exception.Message
        );
}
