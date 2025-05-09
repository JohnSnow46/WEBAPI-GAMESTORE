using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GenresDto;
public class GenreUpdateDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("parentGenreId")]
    public Guid? ParentGenreId { get; set; }
}
