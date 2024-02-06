using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ToolsManager.Abstractions.Models;
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
            
            List<UploadedTool> uploadedTools = new();
            List<(Error, ToolFileInfo)> errors = new();
            foreach (var file in files)
            {
                ToolFileInfo info = new()
                {
                    UserId = claimsPrincipal.Identity!.Name!,
                    Key = file.Name,
                    Name = Path.GetFileNameWithoutExtension(file.FileName),
                    Extension = Path.GetExtension(file.FileName)
                };
                
                var result =
                    await toolsService.UploadNewTool(file.OpenReadStream(), info, cancellationToken);

                if (result.IsSuccess)
                    uploadedTools.Add(result.Value!);
                else
                    errors.Add((result.Error, info));
            }

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
}