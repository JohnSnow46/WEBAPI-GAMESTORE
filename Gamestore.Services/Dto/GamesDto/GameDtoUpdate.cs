using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GamesDto;
public class GameDtoUpdate
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("price")]
    public double Price { get; set; }

    [JsonPropertyName("unitInStock")]
    public int UnitInStock { get; set; }

    [JsonPropertyName("discontinued")]
    public int Discontinued { get; set; }

    [JsonIgnore]
    public List<Guid>? GenreIds { get; set; }

    [JsonIgnore]
    public List<Guid>? PlatformIds { get; set; }

    [JsonIgnore]
    public Guid? PublisherId { get; set; }
}
