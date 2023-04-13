using Gamebot.Helper;
using Microsoft.AspNetCore.Components;
using TwitchLib.Api;
using TwitchLib.Client;

namespace Gamebot.Pages;

public partial class Index
{
    TwitchOptions _options = new();

    [Inject]
    private TwitchAPI _twitchApi { get; set; }

    [Inject]
    private TwitchClient _twitchClient { get; set; }

    protected override void OnInitialized()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        config.GetSection(TwitchOptions.Twitch).Bind(_options);
    }

    private void NavigateToAuthorization()
    {
        var authUrl = Helpers.GetAuthorizationCodeUrl(
            _options.ClientId,
            _options.RedirectUri,
            Helpers.GetScopes()
        );
        NavigationManager.NavigateTo(authUrl);
    }
}
