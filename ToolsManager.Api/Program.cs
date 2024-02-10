using ToolsManager.Api.Endpoints;
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

app.MapPost("/api/tools/new", ToolEndpoints.UploadNewTool)
    .DisableAntiforgery()  // need to change that https://github.com/dotnet/aspnetcore/issues/51052
    .RequireAuthorization();

app.MapGet("/api/tools", ToolEndpoints.GetUserTools);

app.Run();