using Microsoft.EntityFrameworkCore;

namespace Gamebot.Persistence;

public class BotDbContext : DbContext
{
    public DbSet<TwitchChannel> Channels { get; set; }

    private string DbPath { get; }

    public BotDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "game.db");
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

public class TwitchChannel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset Added { get; set; }
};