using System.Collections.Concurrent;
using Gamebot.Helper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using ZiggyCreatures.Caching.Fusion;

namespace Gamebot;

public class TwitchClientWorkerService : IHostedService
{
    private TwitchClient _twitchClient;
    private readonly API _api;

    private ConcurrentDictionary<string, string> CacheKeys { get; set; }

    public TwitchClientWorkerService(IHostApplicationLifetime appLifetime, API api)
    {
        _api = api;
        CacheKeys = new();
        appLifetime.ApplicationStarted.Register(OnStarted);
        //appLifetime.ApplicationStopping.Register(OnStopping);
        //appLifetime.ApplicationStopped.Register(OnStopped);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _twitchClient = Bot.CreateTwitchClient();
        Log.Information("Starting - Twitchbot status {@Status}", _twitchClient.IsConnected);
        _twitchClient.OnChatCommandReceived += async (s, e) => await OnChatCommandReceived(s, e);
        _twitchClient.OnConnected += Client_OnConnected;
        _twitchClient.Connect();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Stopping - Twitchbot status {@Status}", _twitchClient.IsConnected);
        _twitchClient.Disconnect();
        return Task.CompletedTask;
    }

    private void OnStarted() =>
        Log.Information("Started host Twitchbot - status {@Status}", _twitchClient.IsConnected);

    private void OnStopping() => Log.Information("3. OnStopping has been called.");

    private void OnStopped() => Log.Information("5. OnStopped has been called.");

    private async Task OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
    {
        if (e.Command.CommandIdentifier is not '!')
            return;

        Log.Information(e.Command.ChatMessage.Message);
        if (e.Command.CommandText.Equals("match"))
        {
            if (CacheKeys.TryGetValue(e.Command.ChatMessage.Channel, out string cachekey))
            {
                var match = await _api.GetMatch(cachekey);
                var matchString = CommandHelper.GetCommandString(match);
                _twitchClient.SendMessage(e.Command.ChatMessage.Channel, matchString);
            }
        }

        if (e.Command.CommandText.Equals("setmatch"))
        {
            if (string.IsNullOrWhiteSpace(e.Command.ArgumentsAsString))
                return;

            var matchLink = await _api.GetMatchLink(e.Command.ArgumentsAsString);

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

    private void Client_OnConnected(object sender, OnConnectedArgs e) =>
        Log.Information($"Connected to {e.BotUsername}");
}
