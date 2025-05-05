namespace Gamestore.Entities;

public class Platform
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public ICollection<GamePlatform>? GamePlatforms { get; set; }
}