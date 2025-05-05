using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto;

public class PlatformDto
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;
}
