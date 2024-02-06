namespace ToolsManager.Abstractions.Models;

public sealed record Error(string Code, string? Description = null)
{
    public static readonly Error None = new(string.Empty);
    
    public static implicit operator Result(Error error) => Result.Failure(error);
}

public sealed record Error<T>(string Code, string? Description = null)
{
    public static readonly Error<T> None = new(string.Empty);
    
    public static implicit operator Result<T>(Error<T> error) => Result<T>.Failure(error);
    public static implicit operator Error(Error<T> error) => new Error(error.Code, error.Description);
}