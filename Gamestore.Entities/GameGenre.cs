namespace Gamestore.Entities;

public class GameGenre
{
    public Guid Id { get; set; }

    public Guid GameId { get; set; }

    public Game Game { get; set; } = null!;

    public Guid GenreId { get; set; }

    public Genre Genre { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
