using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PublishersDto;
public class PublisherDto
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? HomePage { get; set; }

    public string? Description { get; set; }
}