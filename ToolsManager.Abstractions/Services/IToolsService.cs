using ToolsManager.Abstractions.Models;

namespace ToolsManager.Abstractions.Services;

public interface IToolsService
{
    ValueTask<UploadedTool> UploadNewTool(Stream toolStream, string userName, CancellationToken cancellationToken);
}