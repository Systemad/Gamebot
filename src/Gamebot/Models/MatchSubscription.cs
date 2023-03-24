namespace Gamebot.Models;

// When fetching a game info from a match link
// save the file to team1-team2-date.htm as have that file as key

public class MatchSubscription
{
    public Match Match { get; set; }
    public bool GameOver { get; set; }
}
