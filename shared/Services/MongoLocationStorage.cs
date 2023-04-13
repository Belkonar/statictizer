using System.IO.Compression;
using MongoDB.Driver;
using shared.Models;

namespace shared.Services;

public class MongoLocationStorage : ILocationStorage
{
    private readonly IMongoDatabase _database;

    public MongoLocationStorage(IMongoDatabase database)
    {
        _database = database;
    }
    
    public async Task<Stream> GetFile(string host, string key)
    {
        if (!await FileExists(host, key))
        {
            // This should only happen when no index.
            throw new FileNotFoundException($"Key {key} not found in host {host}");
        }
        
        var collection = _database.GetCollection<ContentFile>("content");

        var filter = Builders<ContentFile>.Filter
            .Eq(x => x.Key, $"{host}:{key}");

        var data = (await collection.Find(filter).FirstAsync())!;

        var stream = new MemoryStream(data.Contents);
        return stream;
    }

    public async Task<bool> FileExists(string host, string key)
    {
        var collection = _database.GetCollection<ContentFile>("content");

        var filter = Builders<ContentFile>.Filter
            .Eq(x => x.Key, $"{host}:{key}");

        return await collection.Find(filter).AnyAsync();
    }

    public async Task UpdateSite(string host, Stream package)
    {
        var collection = _database.GetCollection<ContentFile>("content");

        var purgeFilter = Builders<ContentFile>.Filter
            .Eq(x => x.Host, host);

        await collection.DeleteManyAsync(purgeFilter);

        await using (package)
        {
            using var folder = new TempFolder();
        
            var archive = folder.GetFile();
            var destination = folder.CreateRandomFolder();

            await using var writer = File.OpenWrite(archive);
            await package.CopyToAsync(writer);
            writer.Close();

            ZipFile.ExtractToDirectory(archive, destination);
            
            var files = Directory.GetFiles(destination, "*", SearchOption.AllDirectories).ToList();

            var contents = new List<ContentFile>();

            foreach (var file in files)
            {
                var key = Path.GetRelativePath(destination, file);

                var contentFile = new ContentFile()
                {
                    Key = $"{host}:{key}",
                    Host = host,
                    Contents = await File.ReadAllBytesAsync(file)
                };
                
                contents.Add(contentFile);
            }

            await collection.InsertManyAsync(contents);
        }
    }
}
