using System.Collections.Concurrent;
using Gamebot.Helper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using ZiggyCreatures.Caching.Fusion;

namespace Gamebot;

public class TwitchClientWorkerService : IHostedService
{
    private readonly ILogger _logger;
    private readonly TwitchClient _twitchClient;
    private readonly API _api;

    private ConcurrentDictionary<string, string> CacheKeys { get; set; }

    public TwitchClientWorkerService(
        ILogger<TwitchClientWorkerService> logger,
        IHostApplicationLifetime appLifetime,
        API api
    )
    {
        _logger = logger;
        _api = api;
        CacheKeys = new();
        _twitchClient = Bot.CreateTwitchClient();
        appLifetime.ApplicationStarted.Register(OnStarted);
        //appLifetime.ApplicationStopping.Register(OnStopping);
        //appLifetime.ApplicationStopped.Register(OnStopped);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("1. StartAsync has been called.");
        _twitchClient.OnChatCommandReceived += async (s, e) => await OnChatCommandReceived(s, e);
        _twitchClient.Connect();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("4. StopAsync has been called.");
        _twitchClient.Disconnect();
        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        _logger.LogInformation("2. OnStarted has been called.");
    }

    private void OnStopping()
    {
        _logger.LogInformation("3. OnStopping has been called.");
    }

    private void OnStopped()
    {
        _logger.LogInformation("5. OnStopped has been called.");
    }

    private async Task OnChatCommandReceived(
        object sender,
        TwitchLib.Client.Events.OnChatCommandReceivedArgs e
    )
    {
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
            if (!string.IsNullOrWhiteSpace(e.Command.CommandText))
            {
                var matchLink = await _api.GetMatchLink(e.Command.ArgumentsAsList[1]);

                if (CacheKeys.TryAdd(e.Command.ChatMessage.Channel, matchLink))
                {
                    var match = await _api.GetMatch(matchLink);
                    var matchString = CommandHelper.GetCommandString(match);
                    _twitchClient.SendMessage(e.Command.ChatMessage.Channel, matchString);
                }
            }
        }
    }
}

/*
    ChannelB wants C9 vs FaZe
    

*/
