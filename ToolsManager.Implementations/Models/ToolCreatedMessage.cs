using ToolsManager.Abstractions.Models;

namespace ToolsManager.Implementations.Models;

public sealed record ToolCreatedMessage(Guid ToolId, ToolFileInfo ToolInfo, IEnumerable<string> ShareWith);