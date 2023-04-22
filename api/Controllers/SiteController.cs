using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
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
    private readonly S3LocationStorage _storage;
    private readonly string _bucketName;

    public SiteController(IMongoDatabase database, IConfiguration config, S3LocationStorage storage)
    {
        _database = database;
        _storage = storage;

        _bucketName = config.GetValue<string>("s3bucket") ?? "";

        if (_bucketName == "")
        {
            throw new Exception("`s3bucket` not configured");
        }
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
    
    // TODO: Make a config to turn this call into a no-op
    // The purpose of doing that is async uploads handled via some other system.
    [HttpPut("upload")]
    public async Task<SimpleValue> UploadSiteFromS3([FromBody] UploadRequest request)
    {
        await _storage.UpdateSite(request.Host);
        
        return new SimpleValue()
        {
            Value = "Upload Complete"
        };
    }

    [HttpGet("pre-sign/{host}")]
    public PreSignResponse GetPreSign([FromRoute] string host)
    {
        using var client = new AmazonS3Client(RegionEndpoint.USEast2);

        var key = $"uploads/{host}";

        return new PreSignResponse()
        {
            Key = key,
            SignedUrl = client.GetPreSignedURL(new GetPreSignedUrlRequest()
            {
                BucketName = _bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(15)
            })
        };
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
