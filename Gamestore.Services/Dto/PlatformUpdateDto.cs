using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto;
public class PlatformUpdateDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
