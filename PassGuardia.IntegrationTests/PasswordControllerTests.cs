using Xunit;

using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Net;

namespace PassGuardia.IntegrationTests;

public class PasswordControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PasswordControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTaskByIdShouldReturnNotFound()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"passwords/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}