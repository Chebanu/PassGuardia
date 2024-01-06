namespace PassGuardia.Contracts.Http;

public enum UpdateRoleAction
{
    Add,
    Remove
}

public class UpdateRole
{
    public string Role { get; init; }
    public UpdateRoleAction Action { get; init; }
}

public class AdminUpdateUserRequest
{
    public string Username { get; init; }
    public UpdateRole[] Roles { get; init; }
}