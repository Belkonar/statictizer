using MongoDB.Bson.Serialization.Attributes;

namespace shared.Models;

public class SiteConfig
{
    [BsonId]
    public string Host { get; set; }

    public string DefaultFile { get; set; } = "index.html";
}
