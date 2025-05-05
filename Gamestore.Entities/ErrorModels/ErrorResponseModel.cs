using System.Text.Json.Serialization;

namespace Gamestore.Entities.ErrorModels;

public class ErrorResponseModel
{
    public string ErrorId { get; set; } = Guid.NewGuid().ToString();

    public string Message { get; set; } = "An unexpected error occurred.";

    public int StatusCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? RetryAfter { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Details { get; set; }
}
