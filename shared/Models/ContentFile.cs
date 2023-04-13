using MongoDB.Bson.Serialization.Attributes;

namespace shared.Models;

public class ContentFile
{
    [BsonId]
    public string Key { get; set; }
    
    public string Host { get; set; }
    
    public byte[] Contents { get; set; }
}
