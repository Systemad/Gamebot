using Serilog;

namespace Gamebot;

public class TwitchClientWorkerService : IHostedService
{
    private TwitchBotClient _twitchBotClient;

    public TwitchClientWorkerService(
        IHostApplicationLifetime appLifetime,
        TwitchBotClient twitchBotClient
    )
    {
        _twitchBotClient = twitchBotClient;
        appLifetime.ApplicationStarted.Register(OnStarted);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("Starting - Twitchbot status {@Status}", _twitchBotClient.BotStatus);
        _twitchBotClient.ConnectBot();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Stopping - Twitchbot status {@Status}", _twitchBotClient.BotStatus);
        _twitchBotClient.DisconnectBot();
        return Task.CompletedTask;
    }

    private void OnStarted() =>
        Log.Information("Started host Twitchbot - status {@Status}", _twitchBotClient.BotStatus);
}
