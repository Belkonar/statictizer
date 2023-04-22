namespace shared.Services;

public interface ILocationStorage
{
    public Task<Stream> GetFile(string host, string key);
    
    public Task<bool> FileExists(string host, string key);

    public Task UpdateSite(string host);
}
