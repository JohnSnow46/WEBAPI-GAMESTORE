namespace Gamestore.Entities;

public class Game
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public double Price { get; set; }

    public int UnitInStock { get; set; }

    public int Discontinued { get; set; }

    public Guid? PublisherId { get; set; }

    public Publisher? Publisher { get; set; }

    public ICollection<GameGenre>? GameGenres { get; set; }

    public ICollection<GamePlatform>? GamePlatforms { get; set; }

    public GameFile? GameFile { get; set; }
}
