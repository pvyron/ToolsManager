using System.Security.Claims;
using ToolsManager.Abstractions.Models;
using ToolsManager.Abstractions.Services;

namespace ToolsManager.Api.Endpoints;

public static class ToolsMethods
{
    public static (ValueTask<Result<UploadedTool>> task, ToolFileInfo info)[] UploadNewToolsTasks(
        this IToolsService toolsService, IFormFileCollection files, ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
    {
        var newToolUploadTasks = new (ValueTask<Result<UploadedTool>> task, ToolFileInfo info)[files.Count];
        for (var i = 0; i < files.Count; i++)
        {
            ToolFileInfo info = new()
            {
                UserId = claimsPrincipal.Identity!.Name!,
                Key = files[i].Name,
                Name = Path.GetFileNameWithoutExtension(files[i].FileName),
                Extension = Path.GetExtension(files[i].FileName)
            };

            newToolUploadTasks[i] = (toolsService.UploadNewTool(files[i].OpenReadStream(), info, cancellationToken),
                info);
        }

        return newToolUploadTasks;
    }

    public static async ValueTask<(List<UploadedTool> uploadedTools, List<(Error, ToolFileInfo)> errors)>
        ResolveUploadNewToolsTasks((ValueTask<Result<UploadedTool>> task, ToolFileInfo info)[] newToolUploadTasks)
    {
        List<UploadedTool> uploadedTools = [];
        List<(Error, ToolFileInfo)> errors = [];
        for (var i = 0; i < newToolUploadTasks.Length; i++)
        {
            var (result, info) = (await newToolUploadTasks[i].task, newToolUploadTasks[i].info);

            if (result.IsSuccess)
                uploadedTools.Add(result.Value!);
            else
                errors.Add((result.Error, info));
        }

        return (uploadedTools, errors);
    }
}