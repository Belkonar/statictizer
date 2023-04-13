using System.CommandLine;
using System.IO.Compression;
using shared;

namespace cli.Handlers;

public class UploadHandler
{
    public async Task Upload(FileInfo config)
    {
        if (!config.Exists)
        {
            throw new FileNotFoundException($"config file at {config} not found");
        }

        var configData = await JsonHelper.GetFile<ConfigFile>(config.FullName);

        var actualTarget = Path.GetFullPath(configData.Target, Path.GetDirectoryName(config.FullName)!);

        if (!Directory.Exists(actualTarget))
        {
            throw new FileNotFoundException($"Directory {actualTarget} doesn't exist");
        }

        using var temp = new TempFolder();

        var targetFile = temp.GetFile();
        
        ZipFile.CreateFromDirectory(actualTarget, targetFile);

        await using var archive = File.OpenRead(targetFile);
        using var client = new HttpClient();

        using var content = new StreamContent(archive);

        var response = await client.PutAsync($"{configData.ApiHost}/site/{configData.SiteHost}", content);

        response.EnsureSuccessStatusCode();
    }
    
    public void Setup(RootCommand rootCommand)
    {
        var fileArgument = new Argument<FileInfo>(
            "config",
            description: "The config to use",
            getDefaultValue: () => new FileInfo("static.json"));
        
        var policyCommand = new Command("upload");

        rootCommand.AddCommand(policyCommand);

        policyCommand.AddArgument(fileArgument);

        policyCommand.SetHandler(async (dir) =>
        {
            await Upload(dir);
        }, fileArgument);
    }
}
