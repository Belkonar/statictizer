namespace shared;

/// <summary>
/// Generates a temporary folder, disposing deletes it
/// </summary>
public class TempFolder : IDisposable
{
    public string Dir { get; }

    public bool ShouldClear = true;
    
    public TempFolder()
    {
        Dir = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());

        if (!Directory.Exists(Dir))
        {
            Directory.CreateDirectory(Dir);
        }
    }

    // this is helpful when using a sub directory
    public TempFolder(string path, bool shouldClear = false)
    {
        ShouldClear = shouldClear;
        Dir = path;
    }

    public string GetFile(string fileName)
    {
        return Path.Join(Dir, fileName);
    }
    
    /// <summary>
    /// Get a file with a randomized name
    /// </summary>
    /// <returns>filepath</returns>
    public string GetFile()
    {
        return GetFile(Guid.NewGuid().ToString());
    }

    public string CreateRandomFolder()
    {
        var name = GetFile();
        
        if (!Directory.Exists(name))
        {
            Directory.CreateDirectory(name);
        }

        return name;
    }
    
    public void Dispose()
    {
        if (ShouldClear && Directory.Exists(Dir))
        {
            Directory.Delete(Dir, true);
        }
        
    }
}
