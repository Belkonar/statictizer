using MongoDB.Driver;
using service_deps.Services;
using shared.Models;
using shared.Services;

namespace serve.Logic;

public class ContentLogic
{
    private readonly IMongoDatabase _database;
    private readonly S3LocationStorage _storage;

    public ContentLogic(IMongoDatabase database, S3LocationStorage storage)
    {
        _database = database;
        _storage = storage;
    }
    
    public async Task<IResult> GetContent(HttpContext http, string key)
    {
        var host = http.Request.Host.Host;

        var config = await GetConfig(host);

        if (config == null)
        {
            return Results.NotFound(new
            {
                Error = $"Site `{host}` not configured"
            });
            throw new FileNotFoundException($"Site {host} not configured");
        }

        if (!await _storage.FileExists(host, key))
        {
            key = config.DefaultFile;
        }
        
        http.Response.Headers.CacheControl = config.DefaultCache;

        try
        {
            return Results.File(await _storage.GetFile(host, key), MimeTypes.GetMimeType(key));
        }
        catch (FileNotFoundException)
        {
            return Results.NotFound(new
            {
                Error = $"Key `{key}` not found on host `{host}`"
            });
        }
    }

    public async Task<SiteConfig?> GetConfig(string host)
    {
        var collection = _database.GetCollection<SiteConfig>("sites");

        var filter = Builders<SiteConfig>.Filter
            .Eq(x => x.Host, host);

        return await collection.Find(filter).FirstOrDefaultAsync();
    }
}
