using Newtonsoft.Json;

namespace Gamestore.Services.Dto.GamesDto;

public class GameRequestDto
{
    public GameDtoCreate Game { get; set; } = null!;

    [JsonProperty("genres")]
#pragma warning disable IDE0028 // Simplify collection initialization
    public List<Guid> Genres { get; set; } = new List<Guid>();

    [JsonProperty("platforms")]
    public List<Guid> Platforms { get; set; } = new List<Guid>();
#pragma warning restore IDE0028 // Simplify collection initialization

    [JsonProperty("publisher")]
    public Guid? Publisher { get; set; }
}
