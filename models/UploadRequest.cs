using System.Text.Json.Serialization;

namespace shared.Models;

public class UploadRequest
{
    [JsonPropertyName("Host")]
    public string Host { get; set; }
}
