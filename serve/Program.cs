using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using serve;
using serve.Logic;
using shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ContentLogic>();

builder.Services.AddSingleton<ILocationStorage, MongoLocationStorage>();

#pragma warning disable 618
//BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
#pragma warning restore

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

builder.Services.AddSingleton<IMongoDatabase>(_ => new MongoClient(
        builder.Configuration.GetValue<string>("MongoUri")
    ).GetDatabase("statictizer")
);

var app = builder.Build();

async Task<IResult> Run(HttpContext http, string slug)
{
    await using var scope = app.Services.CreateAsyncScope();
    var logic = scope.ServiceProvider.GetRequiredService<ContentLogic>();
    
    return await logic.GetContent(http, slug);
}

// This is needed for LBs to work properly
// It's the only file you can't have mapped to a static file
app.MapGet("/system-health", () => new
{
    status = "ok"
});

// Need to specify `handler` or it borks
app.MapGet("/", handler: async (HttpContext context) => await Run(context, ""));
app.MapGet("/{*slug}", async (HttpContext context, [FromRoute] string slug) => await Run(context, slug));

app.Run();
