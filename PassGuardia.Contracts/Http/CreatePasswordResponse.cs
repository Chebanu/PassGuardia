namespace PassGuardia.Contracts.Http;

public class CreatePasswordResponse
{
    public Guid PasswordId { get; init; }
    public bool Success { get; init; }
    public string[] Errors { get; init; }
}