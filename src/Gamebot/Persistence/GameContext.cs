using Gamebot.Models;
using Microsoft.EntityFrameworkCore;

namespace Gamebot.Persistence;

public class GameContext : DbContext
{
    public DbSet<MatchSubscription> ActiveMatches { get; set; }
    public DbSet<Match> Matches { get; set; }

    public string DbPath { get; }

    public GameContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "blogging.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MatchSubscription>().HasIndex(m => m.Channel).IsUnique();
        //        modelBuilder.Entity<MatchSubscription>().HasKey(m => m.Channel);
        //modelBuilder.Entity<Match>().HasKey(m => m.MatchLink);
        modelBuilder.Entity<Match>().HasIndex(m => m.MatchLink).IsUnique();

        modelBuilder.Entity<MatchSubscription>().HasOne<Match>();
        base.OnModelCreating(modelBuilder);
    }
}
