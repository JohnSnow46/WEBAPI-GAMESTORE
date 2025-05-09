namespace Gamestore.Services.Dto.GamesDto;
public class GameGetWithNames
{
    public GameDetailsDto Game { get; set; } = null!;

    public List<string>? Genres { get; set; }

    public List<string>? Platforms { get; set; }

    public string? Publisher { get; set; }
}
