using ToolsManager.Abstractions.Models;

namespace ToolsManager.Abstractions.Services;

public interface IToolsService
{
    ValueTask<Result<UploadedTool>> UploadNewTool(Stream toolStream, ToolFileInfo info, CancellationToken cancellationToken);
}