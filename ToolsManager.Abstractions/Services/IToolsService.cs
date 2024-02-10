using ToolsManager.Abstractions.Models;

namespace ToolsManager.Abstractions.Services;

public interface IToolsService
{
    ValueTask<Result<UploadedTool>> UploadNewTool(Stream toolStream, ToolFileInfo info, CancellationToken cancellationToken);
    Task<Result<List<ToolInfo>>> GetUserTools(string userId,
        CancellationToken cancellationToken);
}