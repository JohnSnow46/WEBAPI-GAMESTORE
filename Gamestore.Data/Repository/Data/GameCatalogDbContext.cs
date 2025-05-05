using Gamestore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repository.Data;

public class GameCatalogDbContext(DbContextOptions<GameCatalogDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games { get; set; }

    public DbSet<Genre> Genres { get; set; }

    public DbSet<Platform> Platforms { get; set; }

    public DbSet<GameGenre> GameGenres { get; set; }

    public DbSet<GamePlatform> GamePlatforms { get; set; }

    public DbSet<Publisher> Publishers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Game
        modelBuilder.Entity<Game>()
            .HasKey(g => g.Id);

        modelBuilder.Entity<Game>()
            .Property(g => g.Name)
            .IsRequired();

        modelBuilder.Entity<Game>()
            .Property(g => g.Key)
            .IsRequired();

        modelBuilder.Entity<Game>()
            .HasIndex(g => g.Key)
            .IsUnique();

        modelBuilder.Entity<Game>()
            .Property(g => g.Price)
            .IsRequired();

        modelBuilder.Entity<Game>()
            .Property(g => g.UnitInStock)
            .IsRequired();

        modelBuilder.Entity<Game>()
            .Property(g => g.Discontinued)
            .IsRequired();

        // Game-Publisher
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Publisher)
            .WithMany(p => p.Games)
            .HasForeignKey(g => g.PublisherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // Publisher
        modelBuilder.Entity<Publisher>()
          .HasKey(p => p.Id);

        modelBuilder.Entity<Publisher>()
            .Property(p => p.CompanyName)
            .IsRequired();

        modelBuilder.Entity<Publisher>()
            .HasIndex(p => p.CompanyName)
            .IsUnique();

        modelBuilder.Entity<GameGenre>()
            .HasKey(gg => new { gg.GameId, gg.GenreId });

        modelBuilder.Entity<GamePlatform>()
            .HasKey(gp => new { gp.GameId, gp.PlatformId });

        // GameGenre
        modelBuilder.Entity<GameGenre>()
            .HasOne(gg => gg.Game)
            .WithMany(g => g.GameGenres)
            .HasForeignKey(gg => gg.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GameGenre>()
            .HasOne(gg => gg.Genre)
            .WithMany(g => g.GameGenres)
            .HasForeignKey(gg => gg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);

        // GamePlatform
        modelBuilder.Entity<GamePlatform>()
            .HasOne(gp => gp.Game)
            .WithMany(g => g.GamePlatforms)
            .HasForeignKey(gp => gp.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GamePlatform>()
            .HasOne(gp => gp.Platform)
            .WithMany(p => p.GamePlatforms)
            .HasForeignKey(gp => gp.PlatformId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
