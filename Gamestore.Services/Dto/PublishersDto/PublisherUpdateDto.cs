using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.PublishersDto;
public class PublisherUpdateDto
{
    [Required]
    public string CompanyName { get; set; } = string.Empty;

    public string? HomePage { get; set; }

    public string? Description { get; set; }
}
