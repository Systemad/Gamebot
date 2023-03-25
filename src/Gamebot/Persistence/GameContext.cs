using Gamebot.Models;
using Microsoft.EntityFrameworkCore;

namespace Gamebot.Persistence;

public partial class GameContext : DbContext
{
    public DbSet<MatchSubscription> ActiveMatches { get; set; }
    public DbSet<Match> Matches { get; set; }

    public string DbPath { get; }

    public GameContext()
    {
        var folder = Directory.GetCurrentDirectory();
        DbPath = Path.Join(folder + "/Persistence", "testbot");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite("Data Source=Persistence\\testbot");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>(
            entity =>
            {
                entity.HasKey(e => e.MatchLink);

                entity.ToTable("Match");
            }
        );

        modelBuilder.Entity<MatchSubscription>(
            entity =>
            {
                entity.HasKey(e => e.Channel);

                entity.ToTable("MatchSub");

                entity
                    .HasOne(d => d.Match)
                    .WithMany(p => p.MatchSubs)
                    .HasForeignKey(d => d.MatchId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            }
        );

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
