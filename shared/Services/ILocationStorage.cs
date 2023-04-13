namespace shared.Services;

public interface ILocationStorage
{
    public Task<Stream> GetFile(string host, string key);
    
    public Task<bool> FileExists(string host, string key);

    /// <summary>
    /// Update the site with the specified storage
    /// </summary>
    /// <param name="host"></param>
    /// <param name="package">A ZIP file represented by a stream.</param>
    /// <returns></returns>
    public Task UpdateSite(string host, Stream package);
}
