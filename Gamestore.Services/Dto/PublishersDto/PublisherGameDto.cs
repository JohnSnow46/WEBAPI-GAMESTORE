using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.PublishersDto;
public class PublisherGameDto
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Key { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public double Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int UnitInStock { get; set; }

    [Required]
    [Range(0, 100)]
    public int Discount { get; set; }

    [Required]
    public Guid PublisherId { get; set; }

    public string PublisherName { get; set; } = string.Empty;
}
