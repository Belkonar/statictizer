using MongoDB.Driver;
using shared.Models;
using shared.Services;

namespace serve.Logic;

public class ContentLogic
{
    private readonly ILocationStorage _storage;
    private readonly IMongoDatabase _database;

    public ContentLogic(ILocationStorage storage, IMongoDatabase database)
    {
        _storage = storage;
        _database = database;
    }
    
    public async Task<IResult> GetContent(HttpContext http, string key)
    {
        var host = http.Request.Host.Host;

        var config = await GetConfig(host);

        if (config == null)
        {
            throw new FileNotFoundException($"Site {host} not configured");
        }
        
        if (!await _storage.FileExists(host, key))
        {
            // TODO: Pull this from site config
            key = config.DefaultFile;
        }

        // http.Response.Headers.ContentType = "text/plain";//MimeTypes.GetMimeType(key);
        
        return Results.File(await _storage.GetFile(host, key), MimeTypes.GetMimeType(key));
    }

    public async Task<SiteConfig?> GetConfig(string host)
    {
        var collection = _database.GetCollection<SiteConfig>("sites");

        var filter = Builders<SiteConfig>.Filter
            .Eq(x => x.Host, host);

        return await collection.Find(filter).FirstAsync();
    }
}
