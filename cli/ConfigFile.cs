namespace cli;

public class ConfigFile
{
    /// <summary>
    /// The folder to upload
    /// </summary>
    public string Target { get; set; } = ".";

    public string ApiHost { get; set; } = "http://localhost:5146";

    /// <summary>
    /// This will get yoinked later
    /// </summary>
    public string SiteHost { get; set; } = "";
    
    public List<string>? PreUpload { get; set; }
}
