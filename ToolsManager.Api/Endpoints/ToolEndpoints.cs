using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ToolsManager.Abstractions.Services;

namespace ToolsManager.Api.Endpoints;

public static class ToolEndpoints
{
    public static async ValueTask<IResult> UploadNewTool(
        [FromServices] IToolsService toolsService,
        [FromForm] IFormCollection formFile,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        try
        {
            var files = formFile.Files;

            if (files.Count == 0)
                return Results.Empty;

            var newToolUploadTasks = toolsService.UploadNewToolsTasks(files, claimsPrincipal, cancellationToken);

            var (uploadedTools, errors) = await ToolsMethods.ResolveUploadNewToolsTasks(newToolUploadTasks);

            if (errors.Count == 0)
                return Results.Ok(uploadedTools);

            if (uploadedTools.Count == 0)
                return Results.BadRequest(errors);

            return Results.BadRequest(new { uploadedTools, errors });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { ex.Message, ex.StackTrace });
        }
    }

    public static async ValueTask<IResult> GetUserTools(
        [FromServices] IToolsService toolsService,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        try
        {
            var tools = await toolsService.GetUserTools(claimsPrincipal.Identity!.Name!, cancellationToken);

            if (tools.IsFailure) return Results.BadRequest(tools.Error.ToString());

            return Results.Ok(tools.Value);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { ex.Message, ex.StackTrace });
        }
    }

    public static async ValueTask<IResult> ShareTool(
        [FromForm] IFormCollection formFile,
        [FromHeader(Name = "Recipients")] string recipientEmails,
        [FromServices] IToolsService toolsService,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        //var uploadResponse = await UploadNewTool(toolsService, formFile, claimsPrincipal, cancellationToken); // perfect use for command
        
        try
        {
            var files = formFile.Files;
            
            if (files.Count == 0)
                return Results.Empty;

            var newToolUploadTasks = toolsService.UploadNewToolsTasks(files, recipientEmails.Split(';').ToArray(), claimsPrincipal, cancellationToken);

            var (uploadedTools, errors) = await ToolsMethods.ResolveUploadNewToolsTasks(newToolUploadTasks);

            if (errors.Count == 0)
                return Results.Ok(uploadedTools);

            if (uploadedTools.Count == 0)
                return Results.BadRequest(errors);

            return Results.BadRequest(new { uploadedTools, errors });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { ex.Message, ex.StackTrace });
        }
        
        // uploads tool
        // send it to other user
        // create the other user
        //return await ValueTask.FromResult(Results.Ok());
    }
}