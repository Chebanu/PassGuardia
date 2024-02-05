using PassGuardia.Contracts.Models;

namespace PassGuardia.Contracts.Http;

public class UpdatePasswordRequest
{
    public Visibility Visibility { get; init; }
    public List<string> ShareableList { get; init; }
}