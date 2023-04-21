using EntityFramework.Exceptions.Common;
using Gamebot.Helper;
using Gamebot.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Client;

namespace Gamebot.Pages;

public partial class Index
{
    TwitchOptions _options = new();
    private User? currentChannel;
    private bool _channelJoined;

    [Parameter]
    [SupplyParameterFromQuery(Name = "code")]
    public string? Code { get; set; }

    [Inject]
    private TwitchAPI _twitchApi { get; set; }

    [Inject]
    private TwitchClient _twitchClient { get; set; }

    [Inject]
    private NavigationManager _navigationManager { get; set; }

    [Inject]
    private IDbContextFactory<BotDbContext> contextFactory { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        config.GetSection(TwitchOptions.Twitch).Bind(_options);

        if (!string.IsNullOrEmpty(Code))
        {
            var authCoe = Code;
            _navigationManager.NavigateTo("/");
            var authTokenResponse = await _twitchApi.Auth.GetAccessTokenFromCodeAsync(
                authCoe,
                _options.ClientSecret,
                _options.RedirectUri
            );

            var user = await _twitchApi.Helix.Users.GetUsersAsync(
                accessToken: authTokenResponse.AccessToken
            );
            currentChannel = user.Users.First();
        }

        // TODO: Remove things from URL without reloading page?
        CheckIfChannelIsJoined();
        await base.OnInitializedAsync();
    }

    private void BeginAuthorization()
    {
        var authUrl = Helpers.GetAuthorizationCodeUrl(
            _options.ClientId,
            _options.RedirectUri,
            Helpers.GetScopes()
        );
        _navigationManager.NavigateTo(authUrl);
    }

    // TODO: async??
    private void CheckIfChannelIsJoined()
    {
        if (currentChannel is null)
            return;
        using var dbcontext = contextFactory.CreateDbContext();
        _channelJoined = dbcontext.Channels.Any(c => c.Id == currentChannel.Id);
        //StateHasChanged();
    }

    private async Task JoinChannelAsync()
    {
        if (currentChannel is not null)
        {
            var channel = new TwitchChannel
            {
                Id = currentChannel.Id,
                Name = currentChannel.Login,
                Added = DateTimeOffset.Now
            };

            _twitchClient.JoinChannel(channel.Name);
            using var dbcontext = contextFactory.CreateDbContext();
            dbcontext.Channels.Add(channel);

            try
            {
                await dbcontext.SaveChangesAsync();
                StateHasChanged();
            }
            catch (UniqueConstraintException e)
            {
                Console.WriteLine("already added");
                throw;
            }
        }
    }

    private async Task LeaveChannelAsync()
    {
        if (currentChannel is not null)
        {
            var channel = new TwitchChannel
            {
                Id = currentChannel.Id,
                //Name = currentChannel.Login,
                //Added = DateTimeOffset.Now
            };
            _twitchClient.LeaveChannel(currentChannel.Login);
            using var dbcontext = contextFactory.CreateDbContext();
            dbcontext.Channels.Remove(channel);
            await dbcontext.SaveChangesAsync();
            StateHasChanged();
        }
    }
}
