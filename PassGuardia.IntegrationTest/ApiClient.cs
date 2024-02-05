using Flurl.Http;

using PassGuardia.Contracts.Http;
using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Commands;
using PassGuardia.Domain.Queries;

namespace PassGuardia.IntegrationTest;

internal interface IApiClient : IDisposable
{
    Task<CreatePasswordResponse> CreatePassword(PasswordRequest passwordRequest, string token);
    Task<PasswordResponse> GetPassword(string passwordId);
    Task<PasswordResponse> GetPassword(string passwordId, string token);
    Task UpdatePassword(string passwordId, UpdatePasswordRequest updPasswordVisibility, string token);
    Task<RegisterUserResponse> RegisterUser(RegisterUserRequest registerUserRequest);
    Task<AuthenticateUserResponse> AuthenticateUser(AuthenticateUserRequest authenticateUserRequest);
    Task<MeResponse> Me(string token);
    Task AdminUpdate(AdminUpdateUserCommand adminUpdateCommand, string token);
    Task<GetAuditLogResult> GetAudit(GetAuditLogQuery query, string token);
}

internal class ApiClient : IApiClient
{
    private readonly FlurlClient _client;

    public ApiClient(HttpClient client)
    {
        _client = new FlurlClient(client);
    }

    public Task AdminUpdate(AdminUpdateUserCommand request, string token)
    {
        return _client
                .Request()
                .AppendPathSegment("users")
                .AppendPathSegment("admin")
                .WithOAuthBearerToken(token)
                .AllowHttpStatus(200)
                .PutJsonAsync(request);
    }

    public Task<AuthenticateUserResponse> AuthenticateUser(AuthenticateUserRequest authenticateUserRequest)
    {
        return _client
            .Request()
            .AppendPathSegment("users")
            .AppendPathSegment("authenticate")
            .AllowHttpStatus(200)
            .PostJsonAsync(authenticateUserRequest)
            .ReceiveJson<AuthenticateUserResponse>();
    }

    public Task<CreatePasswordResponse> CreatePassword(PasswordRequest passwordRequest, string token)
    {
        return _client
            .Request()
            .AppendPathSegment("passwords")
            .WithOAuthBearerToken(token)
            .AllowHttpStatus(204)
            .PostJsonAsync(passwordRequest)
            .ReceiveJson<CreatePasswordResponse>();
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public Task<GetAuditLogResult> GetAudit(GetAuditLogQuery query, string token)
    {
        return _client
            .Request()
            .AppendPathSegment("audit")
            .WithOAuthBearerToken(token)
            .AllowHttpStatus(200)
            .GetJsonAsync<GetAuditLogResult>();
    }

    public Task<PasswordResponse> GetPassword(string passwordId)
    {
        return _client
            .Request()
            .AppendPathSegment("passwords")
            .AppendPathSegment(passwordId)
            .AllowHttpStatus(200)
            .GetJsonAsync<PasswordResponse>();
    }

    public Task<PasswordResponse> GetPassword(string passwordId, string token)
    {
        return _client
            .Request()
            .AppendPathSegment("passwords")
            .AppendPathSegment(passwordId)
            .WithOAuthBearerToken(token)
            .AllowHttpStatus(200)
            .GetJsonAsync<PasswordResponse>();
    }
    public Task UpdatePassword(string passwordId, UpdatePasswordRequest updPasswordVisibility, string token)
    {
        return _client
            .Request()
            .AppendPathSegment("passwords")
            .AppendPathSegment(passwordId)
            .WithOAuthBearerToken(token)
            .AllowHttpStatus(204)
            .PutJsonAsync(updPasswordVisibility);
    }

    public Task<MeResponse> Me(string token)
    {
        return _client
            .Request()
            .AppendPathSegment("users")
            .AppendPathSegment("me")
            .WithOAuthBearerToken(token)
            .AllowHttpStatus(200)
            .GetJsonAsync<MeResponse>();
    }

    public Task<RegisterUserResponse> RegisterUser(RegisterUserRequest registerUserRequest)
    {
        return _client
            .Request()
            .AppendPathSegment("users")
            .AllowHttpStatus(201)
            .PostJsonAsync(registerUserRequest)
            .ReceiveJson<RegisterUserResponse>();
    }
}