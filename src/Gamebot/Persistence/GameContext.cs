using Gamebot.Models;
using Microsoft.EntityFrameworkCore;

namespace Gamebot.Persistence;

public class GameContext : DbContext
{
    public DbSet<MatchSubscription> ActiveMatches { get; set; }

    public string DbPath { get; }

    public GameContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "blogging.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseSqlite($"Data Source={DbPath}");
}
