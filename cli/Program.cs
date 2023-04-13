using System.CommandLine;
using Amazon.Runtime.Internal;
using cli.Handlers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<UploadHandler>();
    })
    .Build();

var uploadHandler = host.Services.GetRequiredService<UploadHandler>();

var rootCommand = new RootCommand("Statictizer CLI");

uploadHandler.Setup(rootCommand);

await rootCommand.InvokeAsync(args);
