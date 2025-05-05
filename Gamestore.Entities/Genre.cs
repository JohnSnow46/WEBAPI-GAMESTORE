namespace Gamestore.Entities;

public class Genre
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? ParentGenreId { get; set; }

    public Genre? ParentGenre { get; set; }

    public ICollection<Genre>? ChildGenres { get; set; }

    public ICollection<GameGenre>? GameGenres { get; set; }
}
