using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GamesDto;
public class GameCreateUpdateDto
{
    [JsonPropertyName("game")]
    public GameDtoUpdate Game { get; set; }

    [JsonPropertyName("genres")]
    public List<Guid>? Genres { get; set; }

    [JsonPropertyName("platforms")]
    public List<Guid>? Platforms { get; set; }

    [JsonPropertyName("publisher")]
    public Guid? Publisher { get; set; }
}