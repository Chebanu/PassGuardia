using PassGuardia.Contracts.Models;

namespace PassGuardia.Contracts.Http;

public class UpdatePasswordVisibilityRequest
{
    public Guid Id { get; init; }
    public Visibility GetVisibility { get; init; }
}