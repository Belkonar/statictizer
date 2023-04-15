using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using service_deps.Services;
using shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#pragma warning disable 618
//BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
#pragma warning restore

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

builder.Services.AddSingleton<IMongoDatabase>(_ => new MongoClient(
        builder.Configuration.GetValue<string>("MongoUri")
    ).GetDatabase("statictizer")
);

var storageType = builder.Configuration.GetValue<string>("storageType") ?? "mongo";

builder.Services.AddSingleton<MongoLocationStorage>();
builder.Services.AddSingleton<S3LocationStorage>();

builder.Services.AddSingleton<StorageFactory>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(p => p.AddPolicy("main", b =>
{
    b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

app.UseCors("main");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
