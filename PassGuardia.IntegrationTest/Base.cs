using Bogus;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PassGuardia.Contracts.Http;

using Xunit;

namespace PassGuardia.IntegrationTest;

internal class UserInfo
{
    public string Id { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
    public string Role { get; init; }
    public string Token { get; init; }
}

public  class Base : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly Faker _faker;
    private protected readonly IApiClient _apiClient;
    private readonly IServiceScope _scope;

    public Base(WebApplicationFactory<Program> factory)
    {
        _apiClient = new ApiClient(factory.CreateClient());
        _scope = factory.Services.CreateScope();
        _faker = new Faker();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _scope?.Dispose();
            _apiClient?.Dispose();
        }
    }

    private protected async Task<UserInfo> CreateTestUser(string role = "user")
    {
        var registrationRequest = new RegisterUserRequest
        {
            Username = $"t_{_faker.Database.Random.Uuid():N}",
            Password = $"Ab1!_{_faker.Internet.Password()}"
        };

        var registrationResponse = await _apiClient.RegisterUser(registrationRequest);

        var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManger<IdentityRole>>();
    }

}
