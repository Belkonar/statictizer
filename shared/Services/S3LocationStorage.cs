using System.IO.Compression;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using shared;
using shared.Services;

namespace service_deps.Services;

public class S3LocationStorage : ILocationStorage
{
    private readonly string _bucketName;
    
    public S3LocationStorage(IConfiguration config)
    {
        _bucketName = config.GetValue<string>("s3bucket") ?? "";

        if (_bucketName == "")
        {
            throw new Exception("`s3bucket` not configured");
        }
    }
    
    public async Task<Stream> GetFile(string host, string key)
    {
        var objectKey = $"{host}/{key}";
        
        using var client = new AmazonS3Client(RegionEndpoint.USEast2);

        try
        {
            return (await client.GetObjectAsync(_bucketName, objectKey)).ResponseStream;
        }
        catch (AmazonS3Exception e)
        {
            throw new FileNotFoundException("404", e);
        }
    }

    public async Task<bool> FileExists(string host, string key)
    {
        var objectKey = $"{host}/{key}";
        
        using var client = new AmazonS3Client(RegionEndpoint.USEast2);

        try
        {
            await client.GetObjectMetadataAsync(_bucketName, objectKey);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// This guy could likely be way more optimized. Big files can (and likely will) cause blockages.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="package"></param>
    public async Task UpdateSite(string host, Stream package)
    {
        var prefix = $"{host}/";
        
        using var folder = new TempFolder();
        
        var destination = folder.CreateRandomFolder();
        
        await using (package)
        {
            var archive = folder.GetFile();
            
            await using var writer = File.OpenWrite(archive);
            await package.CopyToAsync(writer);
            writer.Close();

            ZipFile.ExtractToDirectory(archive, destination);
        }
        
        var files = Directory.GetFiles(destination, "*", SearchOption.AllDirectories)
            .ToList();
        
        using var client = new AmazonS3Client(RegionEndpoint.USEast2);

        var objects = await client.ListObjectsV2Async(new ListObjectsV2Request()
        {
            BucketName = _bucketName,
            Prefix = prefix
        });

        var keys = objects.S3Objects.Select(x => x.Key).ToList();
        var newKeys = files.Select(file => $"{prefix}{Path.GetRelativePath(destination, file)}").ToList();

        var purge = keys.Except(newKeys).ToList();

        await client.DeleteObjectsAsync(new DeleteObjectsRequest()
        {
            BucketName = _bucketName,
            Objects = purge.Select(x => new KeyVersion()
            {
                Key = x
            }).ToList()
        });

        using var semaphore = new SemaphoreSlim(10);

        var tasks = new List<Task>();

        // The warnings here are meaningless due to the WhenAll
        foreach (var file in files)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();

                var request = new PutObjectRequest()
                {
                    BucketName = _bucketName,
                    Key = $"{prefix}{Path.GetRelativePath(destination, file)}",

                    FilePath = file
                };

                await client.PutObjectAsync(request);
            
                semaphore.Release();
            }));
        }

        await Task.WhenAll(tasks);
    }
}
