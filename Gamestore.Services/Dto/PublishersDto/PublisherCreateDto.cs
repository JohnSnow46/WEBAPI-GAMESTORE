using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.PublishersDto;
public class PublisherCreateDto
{
    [JsonIgnore]
    public string Id { get; set; }

    public string CompanyName { get; set; }

    public string Description { get; set; }

    public string HomePage { get; set; }
}
