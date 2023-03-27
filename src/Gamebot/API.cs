using AngleSharp.Dom;
using Gamebot.Helper;
using Gamebot.Models;
using ZiggyCreatures.Caching.Fusion;

namespace Gamebot;

public class API
{
    private readonly IFusionCache _fusionCache;

    public API(IFusionCache fusionCache)
    {
        _fusionCache = fusionCache;
    }

    public async Task<string> GetMatchLink(string shortenedTeamName)
    {
        var actualTeamName = TeamsHelper.GetFullNameFromTeamCode(shortenedTeamName);
        if (string.IsNullOrEmpty(actualTeamName))
            return string.Empty;

        var parser = new ContentParser();

        var allMatches = await _fusionCache.GetOrSetAsync<IDocument>(
            "matches",
            _ => parser.DownloadMatches(),
            options => options.SetDuration(TimeSpan.FromHours(6))
        );

        var matchCacheKey = await _fusionCache.GetOrSetAsync<string>(
            actualTeamName, // TODO: Figure out better cache key
            _ => parser.GetMatchLink(allMatches, actualTeamName),
            options => options.SetDuration(TimeSpan.FromHours(3))
        );

        if (string.IsNullOrEmpty(matchCacheKey))
            return string.Empty;

        return matchCacheKey;
    }

    public async Task<Match> GetMatch(string matchLink)
    {
        var parser = new ContentParser();

        var matchResponse = await _fusionCache.GetOrSetAsync<Match>(
            matchLink,
            _ => parser.ParseToMatchObject(matchLink),
            options => options.SetDuration(TimeSpan.FromMinutes(15))
        );
        return matchResponse;
    }
}
