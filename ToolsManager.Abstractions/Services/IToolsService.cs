using ToolsManager.Abstractions.Models;

namespace ToolsManager.Abstractions.Services;

public interface IToolsService
{
    ValueTask<Result<UploadedTool>> UploadNewTool(Stream toolStream, ToolFileInfo info, CancellationToken cancellationToken);
    ValueTask<Result<UploadedTool>> UploadNewTool(Stream toolStream, ToolFileInfo info, IEnumerable<string> shareWith, CancellationToken cancellationToken);
    Task<Result<List<ToolInfo>>> GetUserTools(string userId,
        CancellationToken cancellationToken);
}