using System.Net;

using FluentAssertions;

using Flurl.Http;

using PassGuardia.Contracts.Http;
using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Constants;

using Shouldly;

using static System.Runtime.InteropServices.JavaScript.JSType;

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
        var createdPassword = await CreateUserAndPassword(password, Roles.User, Visibility.Private);

        createdPassword.Should().NotBeNull();
        createdPassword.PasswordId.Should().ShouldNotBeNull();
        createdPassword.Success.Should().BeTrue();
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
        var createdPassword = await CreateUserAndPassword(password, Roles.Admin, Visibility.Private);

        createdPassword.Should().NotBeNull();
        createdPassword.PasswordId.Should().ShouldNotBeNull();
        createdPassword.Success.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Your password must be in the 1-100 character range")]
    [InlineData(null, "The field is null")]
    [InlineData("dzPVGfry7Qfdbri3bvz73ro0VGj5d4GcUC5NMOsRx6hOUtDbXq4qrmap41BXN9h6gG6TuvgKcV5MdeACABQ1MYx8BQnLNaX1Me7cXKlBu8VQex3dwQO0HpPBClHlEHUyNegLQOoQbFkgX1X2c8rwozu2jqWkw5peTEmfHdMs6BOZKpVmS5Pg1nZ5rgB3v8S2AOcn9eHQBJ8d5A5RnphrT9azfoUJyAUERgVzC99lK3HBXApPa8ugj1q54BIeuggLu2c2", "Your password must be in the 1-100 character range")]
    public async Task CreatePasswordShouldBeBadRequest(string password, params string[] errors)
    {
        CreatePasswordResponse result = new();

        try
        {
            result = await CreateUserAndPassword(password, Roles.User, Visibility.Private);

            Assert.Fail("Should have thrown FlurtHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Success.Should().BeFalse();
            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().HaveCount(1);
            errorResult.Errors.Should().BeEquivalentTo(errors);
        }
    }

    [Fact]
    public async Task CreateShareablePasswordWithNullSharedListShouldReturnBadRequest()
    {
        try
        {
            var result = await CreateUserAndPassword("password", Roles.User, Visibility.Shared);

            Assert.Fail("Should have thrown FlurtHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().HaveCount(1);
            errorResult.Errors.Should().BeEquivalentTo("With 'Shared' visibility the shared list should not be empty");
        }
    }

    [Fact]
    public async Task CreateShareablePasswordWithNonEmptyListOfSharedUsernamesShouldDoIt()
    {
        var user1 = await CreateTestRole();
        var user2 = await CreateTestRole();

        var sharedList = new List<string>
        {
            user1.Username,
            user2.Username
        };

        var result = await CreateUserAndPassword("password", sharedList, Roles.User, Visibility.Shared);

        result.Should().NotBeNull();
        result.PasswordId.Should().ShouldNotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateShareablePasswordWithEmptyListShouldReturnBadRequest()
    {
        var usernames = new List<string>();
        CreatePasswordResponse result = new();

        try
        {
            result = await CreateUserAndPassword("password", usernames, Roles.User, Visibility.Shared);

            Assert.Fail("Should have thrown FlurtHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().HaveCount(1);
            errorResult.Errors.Should().BeEquivalentTo("The list must not be empty.");
            result.Success.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CreateShareablePasswordAndShareToTheOwnerShouldReturnBadRequest()
    {
        var user = await CreateTestRole();
        var usernames = new List<string>();
        CreatePasswordResponse result = new();

        try
        {
            result = await _apiClient.CreatePassword(new PasswordRequest
            {
                Password = "HelloWorld",
                Visibility = Visibility.Shared,
                ShareableList = usernames
            }, user.Token);

            Assert.Fail("Should have thrown FlurtHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().HaveCount(1);
            errorResult.Errors.Should().BeEquivalentTo("The list must not be empty.");
            result.Success.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CreateShareablePasswordToNonExistentUserShouldReturnBadRequest()
    {
        string user = $"test_{_faker.Database.Random.Uuid()}".Replace("-", "").Substring(0, 15);
        var sharedList = new List<string> { user };
        CreatePasswordResponse result = new();

        try
        {
            result = await CreateUserAndPassword("password", sharedList, Roles.User, Visibility.Shared);

            Assert.Fail("Should have thrown FlurtHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().HaveCount(1);
            errorResult.Errors.Should().BeEquivalentTo($"Non-existent users: {user}");
            result.Success.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData("qwe", "'Shareable List' must be between 5 and 20 characters. You entered 3 characters.")]
    [InlineData("qwertyuiopasdfghjklqwe", "'Shareable List' must be between 5 and 20 characters. You entered 22 characters.")]
    public async Task CreateShareablePasswordToIllegalUsernameShouldReturnBadRequest(string username, params string[] errors)
    {
        var sharedList = new List<string> { username };
        CreatePasswordResponse result = new();

        try
        {
            result = await CreateUserAndPassword("password", sharedList, Roles.User, Visibility.Shared);

            Assert.Fail("Should have thrown FlurtHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().HaveCount(1);
            errorResult.Errors.Should().BeEquivalentTo(errors);
            result.Success.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CreateShareablePasswordWithDuplicateUsernameInShareableListShouldReturnBadRequest()
    {
        var user = await CreateTestRole();
        var usernames = new List<string> { user.Username, user.Username };

        try
        {
            var createPassword = await CreateUserAndPassword("password", usernames, Roles.User, Visibility.Shared);

            Assert.Fail("Should have thrown FlurtHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().HaveCount(1);
            errorResult.Errors.Should().BeEquivalentTo($"Duplicate usernames: {user.Username}, {usernames.Count} times");
        }
    }

    #endregion

    #region GetPassword
    [Theory]
    [InlineData(";D")]
    public async Task GetPublicPasswordForAuthUserShouldDoIt(string password)
    {
        var createdPassword = await CreateUserAndPassword(password, Roles.User, Visibility.Public);

        var user2 = await CreateTestRole(Roles.User);

        var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString(), user2.Token);

        getPassword.Should().NotBeNull();
        getPassword.Password.Should().Be(password);
    }

    [Theory]
    [InlineData("^_~")]
    public async Task GetPublicPasswordForAnonymousShouldDoIt(string password)
    {
        var createdPassword = await CreateUserAndPassword(password, Roles.User, Visibility.Public);

        var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString());

        getPassword.Should().NotBeNull();
        getPassword.Password.Should().Be(password);
    }

    [Theory]
    [InlineData("^_^", "Password Not Found Or Forbidden To Access")]
    public async Task GetPrivatePasswordForAnotherUserShouldGetException(string password, params string[] errors)
    {
        var createdPassword = await CreateUserAndPassword(password, Roles.User, Visibility.Private);

        var user2 = await CreateTestRole(Roles.User);

        try
        {
            var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString(), user2.Token);

            Assert.Fail("Shoulld have thrown FlurtHttpException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().HaveCount(1);
            errorResult.Errors.Should().BeEquivalentTo(errors);
        }
    }

    [Theory]
    [InlineData("^_^", "Password Not Found Or Forbidden To Access")]
    public async Task GetPrivatePasswordForAnAuthorizatedUserShouldGetException(string password, params string[] errors)
    {
        var createdPassword = await CreateUserAndPassword(password, Roles.User, Visibility.Private);

        try
        {
            var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString());

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().HaveCount(1);
            errorResult.Errors.Should().BeEquivalentTo(errors);
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

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().BeEquivalentTo("Password Not Found Or Forbidden To Access");
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

    [Fact]
    public async Task GetSharedPasswordToUserFromSharedListShouldDoIt()
    {
        var user = await CreateTestRole(Roles.User);

        var usernames = new List<string> { user.Username };

        var createdPassword = await CreateUserAndPassword("password", usernames, Roles.User, Visibility.Shared);

        var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString(), user.Token);

        getPassword.Should().NotBeNull();
        getPassword.Password.Should().Be("password");
    }

    [Fact]
    public async Task GetOwnSharedPasswordShouldDoIt()
    {
        var user = await CreateTestRole();
        var user2 = await CreateTestRole();

        var usernames = new List<string> { user2.Username };

        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = "password",
            Visibility = Visibility.Shared,
            ShareableList = usernames
        }, user.Token);

        var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString(), user.Token);

        getPassword.Should().NotBeNull();
        getPassword.Password.Should().Be("password");
    }

    [Fact]
    public async Task GetPasswordToUserWhoIsNotFromSharedListShouldReturnBadRequest()
    {
        var user = await CreateTestRole();

        var usernames = new List<string> { user.Username };

        var createdPassword = await CreateUserAndPassword("password", usernames, Roles.User, Visibility.Shared);

        try
        {
            var getPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString());

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().HaveCount(1);
            errorResult.Errors.Should().BeEquivalentTo("Password Not Found Or Forbidden To Access");
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
            Visibility = Visibility.Private
        }, user.Token);

        var updRequest = new UpdatePasswordRequest { Visibility = Visibility.Public };

        await _apiClient.UpdatePassword(createdPassword.PasswordId.ToString(), updRequest, user.Token);

        var getUpdatedPassword = await _apiClient.GetPassword(createdPassword.PasswordId.ToString());

        getUpdatedPassword.Visibility.Should().Be(updRequest.Visibility);
    }

    [Theory]
    [InlineData("helloWorld", "Access denied to other users")]
    public async Task UpdatePasswordWithNonOwnerRoleUserShouldThrownException(string password, params string[] errors)
    {
        var user = await CreateTestRole(Roles.User);
        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            Visibility = Visibility.Private
        }, user.Token);

        var updRequest = new UpdatePasswordRequest { Visibility = Visibility.Public };

        var user2 = await CreateTestRole(Roles.User);

        try
        {
            await _apiClient.UpdatePassword(createdPassword.PasswordId.ToString(), updRequest, user2.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().BeEquivalentTo(errors);
        }
    }

    [Theory]
    [InlineData("helloWorld", "Access denied to other users")]
    public async Task UpdatePasswordWithNonOwnerRole_Admin_ShouldThrownException(string password, params string[] errors)
    {
        var user = await CreateTestRole(Roles.User);
        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            Visibility = Visibility.Private
        }, user.Token);

        var updRequest = new UpdatePasswordRequest { Visibility = Visibility.Public };

        var admin = await CreateTestRole(Roles.User);

        try
        {
            await _apiClient.UpdatePassword(createdPassword.PasswordId.ToString(), updRequest, admin.Token);
            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().BeEquivalentTo(errors);
        }
    }

    [Theory]
    [InlineData("helloWorld", "Visibility is the same as the previous one")]
    public async Task UpdatePasswordVisibilityOfTheSameVisibilityShouldReturnBadRequest(string password, params string[] errors)
    {
        var user = await CreateTestRole(Roles.User);
        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            Visibility = Visibility.Private
        }, user.Token);

        try
        {
            var updRequest = new UpdatePasswordRequest { Visibility = Visibility.Private };

            await _apiClient.UpdatePassword(createdPassword.PasswordId.ToString(), updRequest, user.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().BeEquivalentTo(errors);
        }
    }

    [Fact]
    public async Task UpdatePasswordShouldReturnBadRequestDueToUnknownGuid()
    {
        var user = await CreateTestRole(Roles.User);

        try
        {
            var updRequest = new UpdatePasswordRequest { Visibility = Visibility.Private };

            await _apiClient.UpdatePassword(Guid.NewGuid().ToString(), updRequest, user.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().BeEquivalentTo("Password doesn't exist");
        }
    }

    [Fact]
    public async Task UpdatePasswordWithSharedVisibilityIsNotAbleToChangeVisibility()
    {
        var owner = await CreateTestRole(Roles.User);
        var user = await CreateTestRole(Roles.User);

        var usernames = new List<string> { user.Username };

        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = "HelloWorld",
            Visibility = Visibility.Shared,
            ShareableList = usernames
        }, owner.Token);

        try
        {
            var updRequest = new UpdatePasswordRequest { Visibility = Visibility.Private };

            await _apiClient.UpdatePassword(createdPassword.PasswordId.ToString(), updRequest, owner.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().BeEquivalentTo("You are not able to change 'Shared' Visibility");
        }
    }

    [Fact]
    public async Task UpdateNonSharedToSharedPasswordShouldReturnBadRequest()
    {
        var owner = await CreateTestRole();

        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = "HelloWorld",
            Visibility = Visibility.Private
        }, owner.Token);

        try
        {
            var updRequest = new UpdatePasswordRequest { Visibility = Visibility.Shared };

            await _apiClient.UpdatePassword(createdPassword.PasswordId.ToString(), updRequest, owner.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().BeEquivalentTo("You are not able to change 'Shared' Visibility");
        }
    }

    [Fact]
    public async Task UpdateSharedPasswordWithUpdatedShareableListShouldDoIt()
    {
        var owner = await CreateTestRole();
        var user1 = await CreateTestRole();
        var user2 = await CreateTestRole();

        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = "HelloWorld",
            Visibility = Visibility.Shared,
            ShareableList = new List<string> { user1.Username }
        }, owner.Token);

        var updRequest = new UpdatePasswordRequest
        {
            Visibility = Visibility.Shared,
            ShareableList = new List<string> { user2.Username, user1.Username }
        };

        await _apiClient.UpdatePassword(createdPassword.PasswordId.ToString(), updRequest, owner.Token);

        var updatedPassword1 = await _apiClient.GetPassword(createdPassword.PasswordId.ToString(), user1.Token);
        var updatedPassword2 = await _apiClient.GetPassword(createdPassword.PasswordId.ToString(), user2.Token);

        updatedPassword1.Password.Should().Be("HelloWorld");
        updatedPassword2.Password.Should().Be("HelloWorld");
    }


    [Fact]
    public async Task UpdateSharedPasswordWithDuplicatesShouldReturnBadRequest()
    {
        var owner = await CreateTestRole();
        var user = await CreateTestRole();

        var usernames = new List<string> { user.Username };

        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = "HelloWorld",
            Visibility = Visibility.Shared,
            ShareableList = usernames
        }, owner.Token);

        usernames.Add(user.Username);

        try
        {
            var updRequest = new UpdatePasswordRequest
            {
                Visibility = Visibility.Shared,
                ShareableList = usernames
            };

            await _apiClient.UpdatePassword(createdPassword.PasswordId.ToString(), updRequest, owner.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().BeEquivalentTo($"Duplicate usernames: {user.Username}, {usernames.Count} times");
        }
    }

    [Fact]
    public async Task UpdateSharedPasswordWithOwnerInShareableListShouldReturnBadRequest()
    {
        var owner = await CreateTestRole();
        var user = await CreateTestRole();

        var usernames = new List<string> { user.Username };

        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = "HelloWorld",
            Visibility = Visibility.Shared,
            ShareableList = usernames
        }, owner.Token);

        usernames.Add(owner.Username);

        try
        {
            var updRequest = new UpdatePasswordRequest
            {
                Visibility = Visibility.Shared,
                ShareableList = usernames
            };

            await _apiClient.UpdatePassword(createdPassword.PasswordId.ToString(), updRequest, owner.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().BeEquivalentTo("Owner cannot be in the shared list");
        }
    }

    [Fact]
    public async Task RemovedUserFromTheShareableListAShouldBeDeniedAccessToThePassword()
    {
        var owner = await CreateTestRole();
        var user = await CreateTestRole();

        var createdPassword = await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = "HelloWorld",
            Visibility = Visibility.Shared,
            ShareableList = new List<string> { user.Username }
        }, owner.Token);

        var updRequest = new UpdatePasswordRequest
        {
            Visibility = Visibility.Shared,
            ShareableList = new List<string>()
        };

        await _apiClient.UpdatePassword(createdPassword.PasswordId.ToString(), updRequest, owner.Token);

        try
        {
            await _apiClient.GetPassword(createdPassword.PasswordId.ToString(), user.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var errorResult = await ex.GetResponseJsonAsync<ErrorResponse>();

            errorResult.Should().NotBeNull();
            errorResult.Errors.Should().BeEquivalentTo("Password Not Found Or Forbidden To Access");
        }
    }
    #endregion

    private async Task<CreatePasswordResponse> CreateUserAndPassword(string password,
                                                                        string role = Roles.User,
                                                                        Visibility visibility = Visibility.Private)
    {
        var user = await CreateTestRole(role);

        return await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            Visibility = visibility
        }, user.Token);
    }

    private async Task<CreatePasswordResponse> CreateUserAndPassword(string password,
                                                                        List<string> usernames,
                                                                        string role = Roles.User,
                                                                        Visibility visibility = Visibility.Private)
    {
        var user = await CreateTestRole(role);

        return await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            Visibility = visibility,
            ShareableList = usernames
        }, user.Token);
    }
}