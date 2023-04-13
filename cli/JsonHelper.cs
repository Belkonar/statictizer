using System.Text.Json;

namespace cli;

public class JsonHelper
{
    public static async Task<T> GetFile<T>(string path)
    {
        var jsonFile = await File.ReadAllTextAsync(path);
        
        // This sure is annoying
        return JsonSerializer.Deserialize<T>(jsonFile, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}
