using System.Net.Http.Json;
using System.Net;

using FluentAssertions;

using PassGuardia.Contracts.Http;
using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Commands;
using PassGuardia.Domain.Constants;

using Shouldly;

using Xunit;
using Flurl.Http;

namespace PassGuardia.IntegrationTest;

public class PasswordTests : Base
{
    public PasswordTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {        
    }

    #region CreatePassword
    [Theory]
    [InlineData("details")]
    [InlineData("détails")]
    [InlineData("细节")]
    [InlineData("details!@#$%^&*()_+")]
    [InlineData("1234567890")]
    [InlineData("details with spaces")]
    [InlineData("details with punctuation.")]
    [InlineData("details with emoji 🤓")]
    public async Task CreateUserPasswordShouldDoIt(string password)
    {
        var createdPassword = await CreatePassword(password, Roles.User, Visibility.Private);

        createdPassword.Should().NotBeNull();
        createdPassword.PasswordId.Should().ShouldNotBeNull();
    }

    [Theory]
    [InlineData("details")]
    [InlineData("détails")]
    [InlineData("细节")]
    [InlineData("details!@#$%^&*()_+")]
    [InlineData("1234567890")]
    [InlineData("details with spaces")]
    [InlineData("details with punctuation.")]
    [InlineData("details with emoji 🤓")]
    public async Task CreateAdminPasswordShouldDoIt(string password)
    {
        var createdPassword = await CreatePassword(password, Roles.Admin, Visibility.Private);

        createdPassword.Should().NotBeNull();
        createdPassword.PasswordId.Should().ShouldNotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("dzPVGfry7Qfdbri3bvz73ro0VGj5d4GcUC5NMOsRx6hOUtDbXq4qrmap41BXN9h6gG6TuvgKcV5MdeACABQ1MYx8BQnLNaX1Me7cXKlBu8VQex3dwQO0HpPBClHlEHUyNegLQOoQbFkgX1X2c8rwozu2jqWkw5peTEmfHdMs6BOZKpVmS5Pg1nZ5rgB3v8S2AOcn9eHQBJ8d5A5RnphrT9azfoUJyAUERgVzC99lK3HBXApPa8ugj1q54BIeuggLu2c2")]
    public async Task CreatePasswordShouldBeBadRequest(string password)
    {
        try
        {
            var result = await CreatePassword(password, Roles.User, Visibility.Private);

            Assert.Fail("Should have thrown FlurtHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Should().NotBeNull();
            result.Errors.Should().HaveCount(1);
        }
    }

    //TODO
    [Theory]
    [InlineData("Unauthorized")]
    public async Task CreatePasswordShouldBeUnauthorized(string password)
    {
        try
        {
            var result = await _apiClient.CreatePassword(new PasswordRequest
            {
                Password = password
            });

            Assert.Fail("Should have thrown FlurtHttpException");
        }
        catch(FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Should().NotBeNull();
        }
    }

    #endregion

    [Theory]
    [InlineData(";D")]
    public async Task GetPublicPasswordForAuthUserShouldDoIt(string password)
    {
        var createdPassword = await CreatePassword(password, Roles.User, Visibility.Public);

        var user2 = await CreateTestUser(Roles.User);

        var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString(), user2.Token);

        getPassword.Should().NotBeNull();
        getPassword.Password.Should().Be(password);
    }

    [Theory]
    [InlineData("^_~")]
    public async Task GetPublicPasswordForAnonymousShouldDoIt(string password)
    {
        var createdPassword = await CreatePassword(password, Roles.User, Visibility.Public);

        var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString());

        getPassword.Should().NotBeNull();
        getPassword.Password.Should().Be(password);
    }

    [Theory]
    [InlineData("^_^")]
    public async Task GetPrivatePasswordForAnotherUserShouldGetException(string password)
    {
        var createdPassword = await CreatePassword(password, Roles.User, Visibility.Private);

        var user2 = await CreateTestUser(Roles.User);

        try
        {
            var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString(), user2.Token);

            Assert.Fail("Shoulld have thrown FlurtHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Should().NotBeNull();
            result.Errors.Should().HaveCount(1);
        }

    }

    private async Task<CreatePasswordResult> CreatePassword(string password, string role = Roles.User, Visibility visibility = Visibility.Private)
    {
        var user = await CreateTestUser(role);

        return  await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            GetVisibility = visibility
        }, user.Token);
    }
}

/*
 * Пароль создать авториз
 * Феил пароль создать без автор
 * 
 * Получить приватный пароль с автор 1
 * Получить публичный пароль с автор 2
 * Получить публичный пароль без автор 0
 * 
 */