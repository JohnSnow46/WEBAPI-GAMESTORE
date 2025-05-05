using System.Text.Json.Serialization;

namespace Gamestore.Services.Dto.GenresDto;
public class GenreDtoCreate
{
    [JsonIgnore]
    public string? Id { get; set; }

    public string? Name { get; set; }

    public Guid? ParentGenreId { get; set; }
}
