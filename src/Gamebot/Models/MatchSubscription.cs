namespace Gamebot.Models;

public partial class MatchSubscription
{
    public string Channel { get; set; } = null!;

    public string MatchId { get; set; } = null!;

    public virtual Match Match { get; set; } = null!;
}
