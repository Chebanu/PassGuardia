using System.Net;

using FluentAssertions;

using Flurl.Http;

using PassGuardia.Contracts.Http;
using PassGuardia.Domain.Constants;

namespace PassGuardia.IntegrationTest;

public class UsersTests : Base
{
    public UsersTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task RegisterAndAuthenticateUserShouldDoIt()
    {
        var registerRequest = new RegisterUserRequest
        {
            Username = $"test_{_faker.Database.Random.Uuid()}".Replace("-", "").Substring(0, 15),
            Password = $"Aa1!_{_faker.Internet.Password()}"
        };
        var registerResponse = await _apiClient.RegisterUser(registerRequest);

        registerResponse.UserId.Should().NotBeNullOrEmpty();

        var authenticateResponse = await _apiClient.AuthenticateUser(new AuthenticateUserRequest
        {
            Username = registerRequest.Username,
            Password = registerRequest.Password
        });

        authenticateResponse.Should().NotBeNull();
        authenticateResponse.Token.Should().NotBeNullOrEmpty();

        var meResponse = await _apiClient.Me(authenticateResponse.Token);

        meResponse.Should().NotBeNull();
        meResponse.Username.Should().Be(registerRequest.Username);
        meResponse.Roles.Should().BeEquivalentTo([Roles.User]);
    }

    [Theory]
    [InlineData(null, "TestPassword123!?", "'Username' must not be empty.")]
    [InlineData("", "TestPassword123!?", "'Username' must not be empty.",
        "'Username' must be between 5 and 20 characters. You entered 0 characters.")]
    [InlineData("TestUser", null, "'Password' must not be empty.")]
    [InlineData("TestUser", "", "'Password' must not be empty.",
        "'Password' must be between 8 and 100 characters. You entered 0 characters.")]
    [InlineData("TestUser", "short", "'Password' must be between 8 and 100 characters. You entered 5 characters.")]
    [InlineData("TestUser", "longnodigitsnouppercase", "Passwords must have at least one non alphanumeric character.",
        "Passwords must have at least one digit ('0'-'9').",
        "Passwords must have at least one uppercase ('A'-'Z').")]
    [InlineData("TestUser", "long123nouppercase", "Passwords must have at least one non alphanumeric character.",
        "Passwords must have at least one uppercase ('A'-'Z').")]
    [InlineData("TestUser", "Long123nospecial", "Passwords must have at least one non alphanumeric character.")]
    [InlineData("TestUser", "Long!nodigits", "Passwords must have at least one digit ('0'-'9').")]
    public async Task RegisterUserShouldReturnBadRequestWhenUsernameOrPasswordIsNullOrEmpty(string username, string password, params string[] errors)
    {
        try
        {
            await _apiClient.RegisterUser(new RegisterUserRequest
            {
                Username = username,
                Password = password
            });

            Assert.Fail("Should have thrown FlurlHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().BeEquivalentTo(errors);
        }
    }

    [Fact]
    public async Task AuthenticateUserShouldReturnBadRequestWhenUserDoesNotExists()
    {
        try
        {
            await _apiClient.AuthenticateUser(new AuthenticateUserRequest
            {
                Username = $"test_{_faker.Database.Random.Uuid()}".Replace("-", "").Substring(0, 15),
                Password = $"Aa1!_{_faker.Internet.Password()}"
            });

            Assert.Fail("Should have thrown FlurlHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().HaveCount(1);
            responseContent.Errors.First().Should().Be("Invalid username or password");
        }
    }

    [Fact]
    public async Task AuthenticateUserShouldReturnBadRequestWhenPasswordIsIncorrect()
    {
        var registerRequest = new RegisterUserRequest
        {
            Username = $"test_{_faker.Database.Random.Uuid()}".Replace("-", "").Substring(0, 15),
            Password = $"Aa1!_{_faker.Internet.Password()}"
        };
        await _apiClient.RegisterUser(registerRequest);

        try
        {
            await _apiClient.AuthenticateUser(new AuthenticateUserRequest
            {
                Username = registerRequest.Username,
                Password = "IncorrectPassword123!?"
            });
            Assert.Fail("Should have thrown FlurlHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().HaveCount(1);
            responseContent.Errors.First().Should().Be("Invalid username or password.");
        }
    }
}