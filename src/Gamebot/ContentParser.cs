using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Gamebot.Models;
using Match = Gamebot.Models.Match;
using MatchType = Gamebot.Models.MatchType;

namespace Gamebot;

public class ContentParser
{
    public async Task<Match> ParseToMatchObject(string matchLink)
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        IDocument document = await context.OpenAsync(matchLink);

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

    public Task<string> GetMatchLink(IDocument document, string team)
    {
        //var config = Configuration.Default.WithDefaultLoader();
        //var context = BrowsingContext.New(config);
        //IDocument document = await context.OpenAsync("https://www.hltv.org/matches");
        var matches = document.QuerySelectorAll(".liveMatch-container");

        foreach (var match in matches)
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

    public async Task<IDocument> DownloadMatches()
    {
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        IDocument document = await context.OpenAsync("https://www.hltv.org/matches");
        return document;
    }
}
