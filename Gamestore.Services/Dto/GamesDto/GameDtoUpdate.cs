using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GamesDto;
public class GameDtoUpdate
{
    public Guid? Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public double Price { get; set; }

    public int UnitInStock { get; set; }

    public int Discontinued { get; set; }

    [JsonIgnore]
    public List<Guid>? GenreIds { get; set; }

    [JsonIgnore]
    public List<Guid>? PlatformIds { get; set; }

    [JsonIgnore]
    public Guid? PublisherId { get; set; }
}
