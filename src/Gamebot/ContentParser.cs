using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Gamebot.Models;
using PuppeteerSharp;
using Match = Gamebot.Models.Match;
using MatchType = Gamebot.Models.MatchType;

namespace Gamebot;

public class ContentParser
{
    public Task<IEnumerable<IElement>> GetAllMatchElements(IDocument document)
    {
        var upComingMatches = document.QuerySelectorAll(".upcomingMatch");
        var liveMatches = document.QuerySelectorAll(".liveMatch-container");
        var allMatches = upComingMatches.Concat(liveMatches);
        return Task.FromResult(allMatches);
    }

    public async Task<Match> ParseToMatchObject(string matchLink)
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        await using var browser = await Puppeteer.LaunchAsync(
            new LaunchOptions { Headless = true }
        );
        await using var page = await browser.NewPageAsync();
        await page.SetUserAgentAsync(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/112.0"
        );
        await page.GoToAsync("https://www.hltv.org/" + matchLink);
        var content = await page.GetContentAsync();

        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        IDocument document = await context.OpenAsync(request => request.Content(content));

        var mapFormat = GetMatchFormat(document);
        var (teamOne, teamTwo, decider) = GetVetos(document);
        var eventObject = GetEvent(document);

        var match = new Match
        {
            MatchLink = matchLink,
            TeamOne = teamOne,
            TeamTwo = teamTwo,
            Decider = decider,
            MatchType = mapFormat,
            Event = eventObject,
        };

        return match;
    }

    private MatchType GetMatchFormat(IParentNode document)
    {
        string preformattedText = document.QuerySelector(".preformatted-text").InnerHtml;
        var matchFormat = preformattedText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .First();
        return matchFormat.Contains("Best of 3") ? MatchType.BO3 : MatchType.BO3;
    }

    private static Event GetEvent(IParentNode document)
    {
        var eventName = document
            .QuerySelector(".timeAndEvent")
            .QuerySelector(".text-ellipsis")
            .QuerySelector("a")
            .InnerHtml;

        string preformattedText = document.QuerySelector(".preformatted-text").InnerHtml;
        var matchFormat = preformattedText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .First();

        var eventObj = new Event
        {
            Name = eventName,
            EventType = matchFormat.Contains("LAN") ? EventType.Online : EventType.Offline
        };
        return eventObj;
    }

    private static (Team, Team, string) GetVetos(IParentNode document)
    {
        var mapBox = document.QuerySelectorAll(".veto-box");
        var mapPicks = mapBox[1].QuerySelector(".padding").TextContent;

        var splitted2 = Regex.Replace(
            mapPicks,
            @"^(?:[\t ]*(?:\r?\n|\r))+",
            string.Empty,
            RegexOptions.Multiline
        );

        string trimmedByLine = string.Join("\n", splitted2.Split('\n').Select(s => s.Trim()));
        var splitted = trimmedByLine.Split("\n");

        var teamList = new List<Team>();
        string decider = string.Empty;

        foreach (var map in splitted)
        {
            if (map.Contains("picked"))
            {
                var splits = Regex.Replace(map.Trim(), @"^\d.", " ").Trim().Split(" picked ");
                var team = new Team { Name = splits[0], Pick = splits[1] };
                teamList.Add(team);
            }

            if (map.Contains("was left over"))
                decider = string.Join(" ", map.Trim().Split(" ").Skip(1));
        }

        var arrayTeam = teamList.ToArray();
        return (arrayTeam[0], arrayTeam[1], decider);
    }

    public Task<string> GetMatchLink(IEnumerable<IElement> elements, string team)
    {
        foreach (var match in elements)
        {
            var matchteams = match.QuerySelectorAll(".matchTeam");
            var team1 = matchteams[0].QuerySelector(".matchTeamName").InnerHtml;
            var team2 = matchteams[1].QuerySelector(".matchTeamName").InnerHtml;

            var comparer = StringComparison.CurrentCultureIgnoreCase;
            var contains = team1.Equals(team, comparer) || team2.Equals(team, comparer);
            if (contains)
            {
                var livematchlink = match.QuerySelector(".liveMatch").QuerySelector("a").Attributes[
                    "href"
                ].Value;
                return Task.FromResult(livematchlink);
            }
        }

        return Task.FromResult(string.Empty);
    }

    public async Task<IEnumerable<IElement>> DownloadMatchesAsync()
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        await using var browser = await Puppeteer.LaunchAsync(
            new LaunchOptions { Headless = true }
        );
        await using var page = await browser.NewPageAsync();
        await page.SetUserAgentAsync(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/112.0"
        );
        await page.GoToAsync("https://www.hltv.org/matches");
        var content = await page.GetContentAsync();

        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        IDocument document = await context.OpenAsync(request => request.Content(content));
        var elements = await GetAllMatchElements(document);
        return elements;
    }
}
