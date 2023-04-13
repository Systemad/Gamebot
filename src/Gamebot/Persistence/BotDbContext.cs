using Microsoft.EntityFrameworkCore;

namespace Gamebot.Persistence;

public class BotDbContext : DbContext
{
    public DbSet<TwitchChannel> Channels { get; set; }

    public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }
}

public class TwitchChannel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset Added { get; set; }
}
