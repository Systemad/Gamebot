﻿using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Match = Gamebot.Models.Match;
using MatchType = Gamebot.Models.MatchType;

namespace Gamebot;

public class API
{
    public async Task<string> SetupMatch(string teamName)
    {
        return "";
    }

    public async Task<Match> FetchMatchInformation(string matchLink)
    {
        var match = new Match
        {
            MatchLink = null,
            TeamOne = null,
            TeamTwo = null,
            Decider = null,
            MatchType = MatchType.BO1,
            Event = null
        };

        return match;
    }

    private async Task<string> GetMapCommand()
    {
        StringBuilder finalString = new();
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var text = File.ReadAllText("/navi-spirit.htm");
        IDocument document = await context.OpenAsync(req => req.Content(text));
        var mapFormat = GetMapFormat(document);
        finalString.Append(mapFormat + ": ");
        var mapPicks = GetMapPickString(document);
        finalString.Append(mapPicks);
        return finalString.ToString();
    }

    private string GetMapFormat(IParentNode document)
    {
        // BO1 or BO3 format
        string preformattedText = document.QuerySelector(".preformatted-text").InnerHtml;
        var matchFormat = preformattedText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .First();
        return matchFormat;
    }

    private string GetMapPickString(IParentNode document)
    {
        var mapString = new StringBuilder();

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

        foreach (var map in splitted)
        {
            if (map.Contains("picked"))
            {
                var splits = Regex.Replace(map.Trim(), @"^\d.", " ").Trim().Split(" picked ");
                var picks = $"{splits[1]} ({splits[0]})";
                mapString.Append(picks);
                mapString.Append(" - ");
            }

            if (map.Contains("was left over"))
                mapString.Append(string.Join(" ", map.Trim().Split(" ").Skip(1)));
        }

        return mapString.ToString();
    }

    private string GetMapLink(IParentNode document)
    {
        var matches = document.QuerySelectorAll(".liveMatch-container");
        // Get list of abbreviations for this team
        List<string> abbrievietion = new() { "Spirit" }; // GetAllNamesForTeam(team);

        foreach (var match in matches)
        {
            var matchteams = match.QuerySelectorAll(".matchTeam");
            var match1 = matchteams[0].QuerySelector(".matchTeamName").InnerHtml;
            var match2 = matchteams[1].QuerySelector(".matchTeamName").InnerHtml;

            var contains = abbrievietion.Contains(match1) || abbrievietion.Contains(match2);
            if (contains)
            {
                var livematchlink = match.QuerySelector(".liveMatch").QuerySelector("a").Attributes[
                    "href"
                ].Value;
                return livematchlink;
            }
        }

        return null;
    }
}
