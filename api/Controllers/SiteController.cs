using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using service_deps.Services;
using shared.Models;
using shared.Services;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class SiteController : ControllerBase
{
    private readonly IMongoDatabase _database;
    private readonly StorageFactory _storageFactory;

    public SiteController(IMongoDatabase database, StorageFactory storageFactory)
    {
        _database = database;
        _storageFactory = storageFactory;
    }
    
    /// <summary>
    /// This is an upsert, but will need to alter it for auth in the future.
    ///
    /// If the thing exists, validate the token first.
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<SiteConfig> CreateConfig([FromBody] SiteConfig config)
    {
        var collection = _database.GetCollection<SiteConfig>("sites");

        var filter = Builders<SiteConfig>.Filter
            .Eq(x => x.Host, config.Host);

        await collection.ReplaceOneAsync(filter, config, new ReplaceOptions()
        {
            IsUpsert = true
        });

        return config;
    }

    [HttpPut("{host}")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadSite([FromRoute] string host)
    {
        var site = await GetSites(host);
        
        var storage = _storageFactory.GetStorage(site.StorageType);
        
        await storage.UpdateSite(host, Request.Body);
        
        return Ok();
    }

    [HttpGet]
    public async Task<List<SiteConfig>> GetSites()
    {
        var collection = _database.GetCollection<SiteConfig>("sites");

        var filter = Builders<SiteConfig>.Filter
            .Empty;

        return await collection.Find(filter).ToListAsync();
    }
    
    [HttpGet("{host}")]
    public async Task<SiteConfig> GetSites([FromRoute] string host)
    {
        var collection = _database.GetCollection<SiteConfig>("sites");

        var filter = Builders<SiteConfig>.Filter
            .Eq(x => x.Host, host);

        return await collection.Find(filter).FirstAsync();
    }

    [HttpDelete("{host}")]
    public async Task Purge([FromRoute] string host)
    {
        var collection = _database.GetCollection<SiteConfig>("sites");
        var contentCollection = _database.GetCollection<ContentFile>("content");
        
        var configFilter = Builders<SiteConfig>.Filter
            .Eq(x => x.Host, host);

        var contentFilter = Builders<ContentFile>.Filter
            .Eq(x => x.Host, host);

        await contentCollection.DeleteManyAsync(contentFilter);
        await collection.DeleteManyAsync(configFilter);
    }
}
