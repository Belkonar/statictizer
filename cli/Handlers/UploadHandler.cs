using System.CommandLine;
using System.IO.Compression;
using System.Net.Http.Json;
using helpers;
using shared;
using shared.Models;

namespace cli.Handlers;

public class UploadHandler
{
    private async Task Upload(FileInfo config)
    {
        if (!config.Exists)
        {
            throw new FileNotFoundException($"config file at {config} not found");
        }

        var configData = await JsonHelper.GetFile<ConfigFile>(config.FullName);

        if (configData.PreUpload is { Count: > 0 })
        {
            ExecutePreUpload(configData.PreUpload);
        }

        var actualTarget = Path.GetFullPath(configData.Target, Path.GetDirectoryName(config.FullName)!);

        if (!Directory.Exists(actualTarget))
        {
            throw new FileNotFoundException($"Directory {actualTarget} doesn't exist");
        }

        using var temp = new TempFolder();

        var targetFile = temp.GetFile();
        
        ZipFile.CreateFromDirectory(actualTarget, targetFile);
        using var client = new HttpClient();

        var signedResponse = await client.GetFromJsonAsync<PreSignResponse>($"{configData.ApiHost}/site/pre-sign/{configData.SiteHost}");
        
        await using var archive = File.OpenRead(targetFile);
        using var content = new StreamContent(archive);

        var response = await client.PutAsync(signedResponse.SignedUrl, content);
        
        response.EnsureSuccessStatusCode();

        var uploadRequest = new UploadRequest()
        {
            Host = configData.SiteHost,
        };

        using var uploadResponse = await client.PutAsJsonAsync($"{configData.ApiHost}/site/upload", uploadRequest);
        
        Console.WriteLine(await uploadResponse.Content.ReadAsStringAsync());

        uploadResponse.EnsureSuccessStatusCode();
    }

    private void ExecutePreUpload(List<string> preUpload)
    {
        var config = new ExecutorConfig()
        {
            Command = preUpload.First(),
            Arguments = preUpload.Skip(1).ToList()
        };

        var response = Executor.Execute(config);
        Console.WriteLine(response.Shared);
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
