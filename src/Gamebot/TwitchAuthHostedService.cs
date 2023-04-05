namespace Gamebot;

/*
public class TwitchAuthHostedService : IHostedService
{
    

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Setup(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }


    private static void ValidateCreds()
    {
        if (string.IsNullOrEmpty(Config.TwitchClientId))
            throw new Exception("client id cannot be null or empty");
        if (string.IsNullOrEmpty(Config.TwitchClientSecret))
            throw new Exception("client secret cannot be null or empty");
        if (string.IsNullOrEmpty(Config.TwitchRedirectUri))
            throw new Exception("redirect uri cannot be null or empty");
        Console.WriteLine(
            $"Using client id '{Config.TwitchClientId}', secret '{Config.TwitchClientSecret}' and redirect url '{Config.TwitchRedirectUri}'."
        );
    }

    private async Task Setup(CancellationToken cancellationToken)
    {
        ValidateCreds();

        // Use out app
        var api = new TwitchLib.Api.TwitchAPI { Settings = { ClientId = Config.TwitchClientId } };

        // create a server to listen for reauest
        var server = new WebServer(Config.TwitchRedirectUri);
        
        // create a url code for user to login and "authorize"
        var authUrl = Helpers.GetAuthorizationCodeUrl(
            Config.TwitchClientId,
            Config.TwitchRedirectUri,
            Helpers.GetScopes()
        );
        Console.WriteLine($"Please authorize here:\n{authUrl}");

        // it returns a unique token
        var auth = await server.Listen();

        //while (!cancellationToken.IsCancellationRequested)
        //{
        
        // which we use to fetch information about this specific user to authorized
        var resp = await api.Auth.GetAccessTokenFromCodeAsync(
            auth.Code,
            Config.TwitchClientSecret,
            Config.TwitchRedirectUri
        );
        api.Settings.AccessToken = resp.AccessToken;

        var user = (await api.Helix.Users.GetUsersAsync()).Users[0];
        Console.WriteLine(
            $"Authorization success!\n\nUser: {user.DisplayName} (id: {user.Id})\nAccess token: {resp.AccessToken}\nRefresh token: {resp.RefreshToken}\nExpires in: {resp.ExpiresIn}\nScopes: {string.Join(", ", resp.Scopes)}"
        );
        var refresh = await api.Auth.RefreshAuthTokenAsync(
            resp.RefreshToken,
            Config.TwitchClientSecret
        );
        api.Settings.AccessToken = refresh.AccessToken;
        var user2 = (await api.Helix.Users.GetUsersAsync()).Users[0];
        Console.WriteLine(
            $"Authorization success!\n\nUser2: {user2.DisplayName} (id: {user2.Id})\nAccess token: {refresh.AccessToken}\nRefresh token: {refresh.RefreshToken}\nExpires in: {refresh.ExpiresIn}\nScopes: {string.Join(", ", refresh.Scopes)}"
        );

        // }
    }
}
 * https://github.com/swiftyspiffy/Twitch-Auth-Example/blob/main/TwitchAuthExample/Program.cs
 * Use this!
 * When person clicks on button to authenticate with twitch
 * take username and id, put them in list
 * then do client.joinchannel(username)
 * and do that on startup as well

*/