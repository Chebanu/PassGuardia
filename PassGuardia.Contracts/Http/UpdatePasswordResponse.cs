using PassGuardia.Contracts.Models;

namespace PassGuardia.Contracts.Http;

public class UpdatePasswordResponse
{
    public Visibility Visibility { get; init; }
    public bool Success { get; init; }
    public IEnumerable<string> Errors { get; init; }
}