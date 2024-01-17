using Flurl.Http;

using PassGuardia.Contracts.Http;
using PassGuardia.Domain.Commands;
using PassGuardia.Domain.Queries;

namespace PassGuardia.IntegrationTest;

internal interface IApiClient : IDisposable
{
    Task<CreatePasswordResult> CreatePassword(PasswordRequest passwordRequest, string token);
    Task<CreatePasswordResult> CreatePassword(PasswordRequest passwordRequest);
    Task<GetPasswordByIdResult> GetPassword(string passwordId);
    Task<GetPasswordByIdResult> GetPassword(string passwordId, string token);
    Task<RegisterUserResponse> RegisterUser(RegisterUserRequest registerUserRequest);
    Task<AuthenticateUserResponse> AuthenticateUser(AuthenticateUserRequest authenticateUserRequest);
    Task<MeResponse> Me(string token);
    Task AdminUpdate(AdminUpdateUserCommand adminUpdateCommand, string token); 
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

    public Task<CreatePasswordResult> CreatePassword(PasswordRequest passwordRequest, string token)
    {
        return _client
            .Request()
            .AppendPathSegment("passwords")
            .WithOAuthBearerToken(token)
            .AllowHttpStatus(201)
            .PostJsonAsync(passwordRequest)
            .ReceiveJson<CreatePasswordResult>();
    }

    public Task<CreatePasswordResult> CreatePassword(PasswordRequest passwordRequest)
    {
        return _client
            .Request()
            .AppendPathSegment("passwords")
            .AllowHttpStatus(201)
            .PostJsonAsync(passwordRequest)
            .ReceiveJson<CreatePasswordResult>();
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    public Task<GetPasswordByIdResult> GetPassword(string passwordId)
    {
        return _client
            .Request()
            .AppendPathSegment("passwords")
            .AppendPathSegment(passwordId)
            .AllowHttpStatus(200)
            .GetJsonAsync<GetPasswordByIdResult>();
    }

    public Task<GetPasswordByIdResult> GetPassword(string passwordId, string token)
    {
        return _client
            .Request()
            .AppendPathSegment("passwords")
            .AppendPathSegment(passwordId)
            .WithOAuthBearerToken(token)
            .AllowHttpStatus(200)
            .GetJsonAsync<GetPasswordByIdResult>();
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