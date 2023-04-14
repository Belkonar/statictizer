using MongoDB.Bson.Serialization.Attributes;

namespace shared.Models;

public class SiteConfig
{
    [BsonId]
    public string Host { get; set; }

    public string DefaultFile { get; set; } = "index.html";

    public string Description { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    /// <example>
    /// `startsWith=asd/`
    /// `ext=.txt`
    /// </example>
    public Dictionary<string, string> Rules { get; set; } = new ();
}
