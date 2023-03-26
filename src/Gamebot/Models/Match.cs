namespace Gamebot.Models;

public partial class Match
{
    public string MatchLink { get; set; } = null!;
    public Team TeamOne { get; set; }
    public Team TeamTwo { get; set; }
    public string Decider { get; set; }
    public MatchType MatchType { get; set; }
    public Event Event { get; set; }
    //public virtual ICollection<MatchSubscription> MatchSubs { get; } =
    //    new List<MatchSubscription>();
}

public class Team
{
    public string Name { get; set; }
    public string Pick { get; set; }
}

public class Event
{
    public string Name { get; set; }
    public EventType EventType { get; set; }
}

public enum MatchType
{
    BO1,
    BO3
}

public enum EventType
{
    Online = 0,
    Offline = 1
}
