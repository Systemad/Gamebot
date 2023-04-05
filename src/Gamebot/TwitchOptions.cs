namespace Gamebot;

public class TwitchOptions
{
    public const string Twitch = "Twitch";

    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string ClientId = string.Empty;
    public string RedirectUri = string.Empty;
    public string ClientSecret = string.Empty;
}
