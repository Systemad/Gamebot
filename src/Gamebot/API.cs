using AngleSharp.Dom;
using Gamebot.Helper;
using Gamebot.Models;
using ZiggyCreatures.Caching.Fusion;

namespace Gamebot;

public class API
{
    private readonly IFusionCache _fusionCache;
    private ContentParser _contentParser;

    public API(IFusionCache fusionCache, ContentParser contentParser)
    {
        _fusionCache = fusionCache;
        _contentParser = contentParser;
    }

    public async Task<string> GetMatchLink(string actualTeamName)
    {
        //var actualTeamName = TeamsHelper.GetFullNameFromTeamCode(shortenedTeamName);
        if (string.IsNullOrEmpty(actualTeamName))
            return string.Empty;

        var allMatches = await _fusionCache.GetOrSetAsync<IEnumerable<IElement>>(
            "matches",
            _ => _contentParser.DownloadMatchesAsync(),
            options => options.SetDuration(TimeSpan.FromHours(1))
        );
        var matchLink = await _contentParser.GetMatchLink(allMatches, actualTeamName);

        /*
        var matchCacheKey = await _fusionCache.GetOrSetAsync<string>(
            actualTeamName, // TODO: Figure out better cache key
            _ => _contentParser.GetMatchLink(allMatches, actualTeamName),
            options => options.SetDuration(TimeSpan.FromHours(1))
        );
        */
        return string.IsNullOrEmpty(matchLink) ? string.Empty : matchLink;
    }

    public async Task<Match> GetMatch(string matchLink)
    {
        var matchResponse = await _fusionCache.GetOrSetAsync<Match>(
            matchLink,
            _ => _contentParser.ParseToMatchObject(matchLink),
            options => options.SetDuration(TimeSpan.FromMinutes(60))
        );
        return matchResponse;
    }
}
