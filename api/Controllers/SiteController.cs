using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using shared.Models;
using shared.Services;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class SiteController : ControllerBase
{
    private readonly IMongoDatabase _database;
    private readonly ILocationStorage _storage;

    public SiteController(IMongoDatabase database, ILocationStorage storage)
    {
        _database = database;
        _storage = storage;
    }

    [HttpPost]
    public async Task<SiteConfig> CreateConfig([FromBody] SiteConfig config)
    {
        var collection = _database.GetCollection<SiteConfig>("sites");
        
        await collection.InsertOneAsync(config);

        return config;
    }

    [HttpPut("{host}")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadSite([FromRoute] string host)
    {
        await _storage.UpdateSite(host, Request.Body);
        return Ok();
    }
}
