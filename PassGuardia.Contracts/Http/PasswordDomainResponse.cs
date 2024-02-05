using PassGuardia.Contracts.Models;

namespace PassGuardia.Contracts.Http;

public class PasswordDomainResponse
{
    public string Password { get; init; }
    public Visibility Visibility { get; init; }
}