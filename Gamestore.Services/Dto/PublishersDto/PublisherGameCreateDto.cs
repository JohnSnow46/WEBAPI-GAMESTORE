using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.PublishersDto;
public class PublisherGameCreateDto
{
    [Required]
    public Guid GameId { get; set; }

    [Required]
    public Guid PublisherId { get; set; }
}
