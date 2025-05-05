namespace Gamestore.Entities;

public class GamePlatform
{
    public Guid Id { get; set; }

    public Guid GameId { get; set; }

    public Game Game { get; set; } = null!;

    public Guid PlatformId { get; set; }

    public Platform Platform { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}