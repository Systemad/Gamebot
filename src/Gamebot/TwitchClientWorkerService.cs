using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;

namespace Gamebot;

public class TwitchClientWorkerService : IHostedService
{
    private readonly ILogger _logger;
    private readonly TwitchClient _twitchClient;

    public TwitchClientWorkerService(
        ILogger<TwitchClientWorkerService> logger,
        IHostApplicationLifetime appLifetime
    )
    {
        _logger = logger;
        _twitchClient = Bot.CreateTwitchClient();
        appLifetime.ApplicationStarted.Register(OnStarted);
        //appLifetime.ApplicationStopping.Register(OnStopping);
        //appLifetime.ApplicationStopped.Register(OnStopped);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("1. StartAsync has been called.");
        _twitchClient.OnMessageReceived += OnMessageReceived;
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

    private void OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.Message.StartsWith("!match"))
        {
            _twitchClient.SendMessage(e.ChatMessage.Channel, "a");
        }
    }
}
