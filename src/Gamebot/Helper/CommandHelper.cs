using Gamebot.Models;

namespace Gamebot.Helper;

public static class CommandHelper
{
    public static string GetCommandString(Match match) =>
        $"{nameof(match.MatchType)} ({match.Event.EventType}) {match.TeamOne.Pick} ({match.TeamOne.Name} - {match.TeamTwo.Pick} ({match.TeamTwo.Name} - {match.Decider}) was left over";
}
