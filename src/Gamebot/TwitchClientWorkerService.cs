using Gamebot.Helper;
using Gamebot.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;

namespace Gamebot;

public class TwitchClientWorkerService : IHostedService
{
    private readonly ILogger _logger;
    private readonly TwitchClient _twitchClient;
    private readonly GameContext _gameContext;

    public TwitchClientWorkerService(
        ILogger<TwitchClientWorkerService> logger,
        IHostApplicationLifetime appLifetime,
        GameContext gameContext
    )
    {
        _logger = logger;
        _gameContext = gameContext;
        _twitchClient = Bot.CreateTwitchClient();
        appLifetime.ApplicationStarted.Register(OnStarted);
        //appLifetime.ApplicationStopping.Register(OnStopping);
        //appLifetime.ApplicationStopped.Register(OnStopped);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("1. StartAsync has been called.");
        _twitchClient.OnMessageReceived += async (s, e) => await OnMessageReceived(s, e);
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

    private async Task OnMessageReceived(
        object sender,
        TwitchLib.Client.Events.OnMessageReceivedArgs e
    )
    {
        if (e.ChatMessage.Message.StartsWith("!match"))
        {
            //if (!string.IsNullOrWhiteSpace(e.ChatMessage.Message))
            //{
            var match = await _gameContext.ActiveMatches.FirstOrDefaultAsync(
                m => m.Channel == e.ChatMessage.Channel
            );
            var cmdString = CommandHelper.GetCommandString(match.Match);
            _twitchClient.SendMessage(e.ChatMessage.Channel, cmdString);
            //}
        }

        if (e.ChatMessage.Message.StartsWith("!setgame"))
        {
            if (!string.IsNullOrWhiteSpace(e.ChatMessage.Message))
            {
                // var hey = await setupgame(streamer, team); return match obj
                /*
                 * setupgame(){
                 *  > Get match link
                 *  > Check if match id exist, return
                 *  > or setup match, with help of link
                 *  > return match
                 * }
                */
                var cmdString = CommandHelper.GetCommandString(match.Match);
                _twitchClient.SendMessage(e.ChatMessage.Channel, cmdString);
            }
        }
    }
}
