using System.Text;
using Microsoft.AspNetCore.Mvc;
using ToolsManager.Abstractions.Services;
using ToolsManager.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.InstallToolsServices();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.ValidateToolsServices();

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
    [FromServices] IToolsService toolsService,
    [FromForm] IFormCollection formFile,
    CancellationToken cancellationToken) =>
{
    try
    {
        if (!authorizationHeader.StartsWith(basic))
            return Results.Unauthorized();

        var credentialsPart = authorizationHeader[6..];

        var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(credentialsPart));

        var credentials = decodedCredentials.Split(':');
        var userName = credentials[0];
        var password = credentials[1];

        var result = await toolsService.UploadNewTool(formFile.Files[0].OpenReadStream(), userName, cancellationToken);

        return Results.Ok($"Uploaded {result}");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new {ex.Message, ex.StackTrace});
    }
}).DisableAntiforgery(); // need to change that https://github.com/dotnet/aspnetcore/issues/51052

app.Run();