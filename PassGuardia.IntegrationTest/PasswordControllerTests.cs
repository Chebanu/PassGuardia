using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using PassGuardia.Contracts.Http;

using Shouldly;

using Xunit;

namespace PassGuardia.IntegrationTest;

public class PasswordControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PasswordControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPasswordByIdShouldReturnNotFound()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"passwords/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("invalid-guid-format")]
    public async Task GetPasswordShouldReturnBadRequestForInvalidId(string id)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"passwords/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("5925c81b-a2fa-4828-9300-65545dc3e8c8")]
    public async Task GetPasswordShouldReturnOkForValidIdFromDatabase(string id)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"passwords/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("37c14091-51f5-4fe3-8bab-c548ecdd78e9")]
    public async Task GetPasswordShouldReturnNotFoundForInvalidIdFromDatabase(string id)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"passwords/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("dzPVGfry7Qfdbri3bvz73ro0VGj5d4GcUC5NMOsRx6hOUtDbXq4qrmap41BXN9h6gG6TuvgKcV5MdeACABQ1MYx8BQnLNaX1Me7cXKlBu8VQex3dwQO0HpPBClHlEHUyNegLQOoQbFkgX1X2c8rwozu2jqWkw5peTEmfHdMs6BOZKpVmS5Pg1nZ5rgB3v8S2AOcn9eHQBJ8d5A5RnphrT9azfoUJyAUERgVzC99lK3HBXApPa8ugj1q54BIeuggLu2c2")]

    public async Task CreatePasswordShouldBeBadRequest(string password)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        var invalidRequest = new PasswordRequest { Password = password };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("passwords", invalidRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("PassGuardia")]
    [InlineData(" ")]
    [InlineData("细节")]
    [InlineData("☆*:.｡. o(≧▽≦)o .｡.:*☆")]
    public async Task CreatePasswordShouldBeCreated(string password)
    {
        // Arrange
        HttpClient client = _factory.CreateClient();
        var validRequest = new PasswordRequest { Password = password };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("passwords", validRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }
}