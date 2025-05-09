namespace Gamestore.Services.Dto.GamesDto;
public class GameDetailsDto
{
    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string? Description { get; set; }

    public double Price { get; set; }

    public int UnitInStock { get; set; }

    public int Discontinued { get; set; }
}
