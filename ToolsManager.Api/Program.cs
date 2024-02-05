using System.Text;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

var basic = "Basic ";

app.MapPost("/api/tools/new", async (
    [FromHeader(Name = "Authorization")] string authorizationHeader, 
    [FromServices] IHttpContextAccessor contextAccessor,
    [FromForm] IFormCollection formFile) =>
{
    if (!authorizationHeader.StartsWith(basic))
        return Results.Unauthorized();

    var credentialsPart = authorizationHeader[6..];

    var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(credentialsPart));

    var credentials = decodedCredentials.Split(':');
    var userName = credentials[0];
    var password = credentials[1];

    var fileName = Guid.NewGuid().ToString();

    var blobClient = new BlobContainerClient(
        "",
        "tools");

    if (!await blobClient.ExistsAsync())
        await blobClient.CreateAsync();

    var tableClient = new TableClient(
        "",
        "tools");
    
    await tableClient.CreateIfNotExistsAsync();
    
    var newBlob = await blobClient.UploadBlobAsync(fileName, formFile.Files[0].OpenReadStream());

    var newTool = tableClient.AddEntity(new ToolTableEntity
    {
        PartitionKey = userName,
        RowKey = fileName
    });
    
    if (newBlob.HasValue && !newTool.IsError)
        return Results.Ok($"Uploaded {Convert.ToBase64String(newBlob.Value.ContentHash)}");

    return Results.BadRequest();
}).DisableAntiforgery(); // need to change that https://github.com/dotnet/aspnetcore/issues/51052

app.Run();


class ToolTableEntity : ITableEntity
{
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}