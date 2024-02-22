using Microsoft.AspNetCore.Mvc;
using ToolsManager.Api.Endpoints;
using ToolsManager.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.InstallToolsAuthentication();
builder.InstallToolsServices();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAntiforgery();

app.ValidateToolsServices();
app.MapToolsAuthentication();

app.MapPost("/api/tools/new", ToolEndpoints.UploadNewTool)
    .DisableAntiforgery()  // need to change that https://github.com/dotnet/aspnetcore/issues/51052
    .RequireAuthorization();

app.MapGet("/api/tools", ToolEndpoints.GetUserTools)
    .RequireAuthorization();

app.MapPost("/api/tools/share", ToolEndpoints.ShareTool)
    .DisableAntiforgery()  // need to change that https://github.com/dotnet/aspnetcore/issues/51052
    .RequireAuthorization();

app.MapPost("/tools/shared", () =>
{

}).RequireAuthorization();

app.MapGet("api/tools/{id:guid}", ([FromRoute] Guid id) =>
{
// downloads tool
}).RequireAuthorization();

app.Run();