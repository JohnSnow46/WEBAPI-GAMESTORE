using System.Text.Json.Serialization;

namespace Gamestore.Entities;

public class GamePlatform
{
    public Guid Id { get; set; }

    public Guid GameId { get; set; }

    [JsonIgnore]
    public Game Game { get; set; } = null!;

    public Guid PlatformId { get; set; }

    [JsonIgnore]
    public Platform Platform { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}