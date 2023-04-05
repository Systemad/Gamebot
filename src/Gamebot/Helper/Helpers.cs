namespace Gamebot.Helper;

public static class Helpers
{
    private static List<string> Scopes = new() { "chat:read", "chat:edit" };
    public static string GetAuthorizationCodeUrl(
        string clientId,
        string redirectUri,
        List<string> scopes
    )
    {
        var scopesStr = string.Join('+', scopes);

        return "https://id.twitch.tv/oauth2/authorize?"
               + $"client_id={clientId}&"
               + $"redirect_uri={System.Web.HttpUtility.UrlEncode(redirectUri)}&"
               + "response_type=code&"
               + $"scope={scopesStr}";
    }

    public static List<string> GetScopes() => Scopes;
}