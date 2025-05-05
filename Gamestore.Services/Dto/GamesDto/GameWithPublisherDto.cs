namespace Gamestore.Services.Dto.GamesDto;
public class GameWithPublisherDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string? Description { get; set; }

    public double Price { get; set; }

    public int UnitInStock { get; set; }

    public int Discount { get; set; }

    public Guid PublisherId { get; set; }

    public string PublisherCompanyName { get; set; } = string.Empty;

    public string? PublisherHomePage { get; set; }
}
