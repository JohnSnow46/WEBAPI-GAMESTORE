namespace Gamestore.Services.Dto.GamesDto;
public class GameCreateUpdateDto
{
    public GameDtoUpdate Game { get; set; } = null!;

    public List<Guid>? Genres { get; set; }

    public List<Guid>? Platforms { get; set; }

    public Guid? Publisher { get; set; }
}