namespace Gamestore.Services.Dto.PublishersDto;
public class PublisherGameListDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public double Price { get; set; }

    public int Discount { get; set; }

    public string PublisherName { get; set; } = string.Empty;
}
