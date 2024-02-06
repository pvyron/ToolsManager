using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ToolsManager.Abstractions.Services;
using ToolsManager.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.InstallToolsServices();
builder.InstallToolsAuthentication();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAntiforgery();

app.ValidateToolsServices();
app.MapToolsAuthentication();



var basic = "Basic ";

app.MapPost("/api/tools/new", async (
    [FromServices] IToolsService toolsService,
    [FromForm] IFormCollection formFile,
    ClaimsPrincipal claimsPrincipal,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userName = claimsPrincipal.Identity!.Name;

        var result = await toolsService.UploadNewTool(formFile.Files[0].OpenReadStream(), userName, cancellationToken);

        return Results.Ok($"Uploaded {result}");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new {ex.Message, ex.StackTrace});
    }
}).DisableAntiforgery().RequireAuthorization(); // need to change that https://github.com/dotnet/aspnetcore/issues/51052

app.Run();