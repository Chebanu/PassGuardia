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
    [InlineData(" ")]
    public async Task CreateUserPasswordShouldDoIt(string password)
    {
        var createdPassword = await CreateUserAndUser_sPassword(password, Roles.User, Visibility.Private);

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
    [InlineData(" ")]
    public async Task CreateAdminPasswordShouldDoIt(string password)
    {
        var createdPassword = await CreateUserAndUser_sPassword(password, Roles.Admin, Visibility.Private);

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
            var result = await CreateUserAndUser_sPassword(password, Roles.User, Visibility.Private);

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

    #endregion


    #region GetPassword
    [Theory]
    [InlineData(";D")]
    public async Task GetPublicPasswordForAuthUserShouldDoIt(string password)
    {
        var createdPassword = await CreateUserAndUser_sPassword(password, Roles.User, Visibility.Public);

        var user2 = await CreateTestRole(Roles.User);

        var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString(), user2.Token);

        getPassword.Should().NotBeNull();
        getPassword.Password.Should().Be(password);
    }

    [Theory]
    [InlineData("^_~")]
    public async Task GetPublicPasswordForAnonymousShouldDoIt(string password)
    {
        var createdPassword = await CreateUserAndUser_sPassword(password, Roles.User, Visibility.Public);

        var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString());

        getPassword.Should().NotBeNull();
        getPassword.Password.Should().Be(password);
    }

    [Theory]
    [InlineData("^_^")]
    public async Task GetPrivatePasswordForAnotherUserShouldGetException(string password)
    {
        var createdPassword = await CreateUserAndUser_sPassword(password, Roles.User, Visibility.Private);

        var user2 = await CreateTestRole(Roles.User);

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

    [Theory]
    [InlineData("^_^")]
    public async Task GetPrivatePasswordForAnAuthorizatedUserShouldGetException(string password)
    {
        var createdPassword = await CreateUserAndUser_sPassword(password, Roles.User, Visibility.Private);

        try
        {
            var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString());

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetNonExistentPasswordShouldGetException()
    {
        try
        {
            var getPassword = await _apiClient.GetPassword(Guid.NewGuid().ToString());

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData("invalid-guid-format")]
    public async Task GetPasswordShouldReturnBadRequestForInvalidId(string id)
    {
        try
        {
            var getPassword = await _apiClient.GetPassword(id);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Should().NotBeNull();
        }
    }

    #endregion

    #region UpdatePasswordVisibility
    [Theory]
    [InlineData("helloWorld")]
    public async Task UpdatePasswordVisibilityShouldDoIt(string password)
    {
        var user = await CreateTestRole(Roles.User);
        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            GetVisibility = Visibility.Private
        }, user.Token);

        var updRequest = new UpdatePasswordVisibilityRequest
        {
            Id = createdPassword.PasswordId,
            GetVisibility = Visibility.Public
        };

        await _apiClient.UpdatePasswordVisibility(updRequest, user.Token);

        var getUpdatedPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString());

        getUpdatedPassword.GetVisibility.Should().Be(updRequest.GetVisibility);
    }

    [Theory]
    [InlineData("helloWorld")]
    public async Task UpdatePasswordWithNonOwnerRole_User_ShouldThrownException(string password)
    {
        var user = await CreateTestRole(Roles.User);
        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            GetVisibility = Visibility.Private
        }, user.Token);

        var updRequest = new UpdatePasswordVisibilityRequest
        {
            Id = createdPassword.PasswordId,
            GetVisibility = Visibility.Public
        };

        var user2 = await CreateTestRole(Roles.User);

        try
        {
            await _apiClient.UpdatePasswordVisibility(updRequest, user2.Token);
            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData("helloWorld")]
    public async Task UpdatePasswordWithNonOwnerRole_Admin_ShouldThrownException(string password)
    {
        var user = await CreateTestRole(Roles.User);
        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            GetVisibility = Visibility.Private
        }, user.Token);

        var updRequest = new UpdatePasswordVisibilityRequest
        {
            Id = createdPassword.PasswordId,
            GetVisibility = Visibility.Public
        };

        var admin = await CreateTestRole(Roles.User);

        try
        {
            await _apiClient.UpdatePasswordVisibility(updRequest, admin.Token);
            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData("helloWorld")]
    public async Task UpdatePasswordVisibilityOfTheSameVisibilityShouldReturnBadRequest(string password)
    {
        var user = await CreateTestRole(Roles.User);
        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            GetVisibility = Visibility.Private
        }, user.Token);

        try
        {
            var updRequest = new UpdatePasswordVisibilityRequest
            {
                Id = createdPassword.PasswordId,
                GetVisibility = Visibility.Private
            };

            await _apiClient.UpdatePasswordVisibility(updRequest, user.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task UpdatePasswordShouldReturnBadRequestDueToUnknownGuid()
    {
        var user = await CreateTestRole(Roles.User);

        try
        {
            var updRequest = new UpdatePasswordVisibilityRequest
            {
                Id = Guid.NewGuid(),
                GetVisibility = Visibility.Private
            };

            await _apiClient.UpdatePasswordVisibility(updRequest, user.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Should().NotBeNull();
        }
    }
    #endregion

    private async Task<CreatePasswordResult> CreateUserAndUser_sPassword(string password, string role = Roles.User, Visibility visibility = Visibility.Private)
    {
        var user = await CreateTestRole(role);

        return await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            GetVisibility = visibility
        }, user.Token);
    }
}