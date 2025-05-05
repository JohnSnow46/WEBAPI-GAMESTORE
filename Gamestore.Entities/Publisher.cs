using System.Text.Json.Serialization;

namespace Gamestore.Entities;
public class Publisher
{
    public Guid Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string HomePage { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Game>? Games { get; set; }
}
