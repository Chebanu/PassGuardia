﻿using System.Net;

using FluentAssertions;

using Flurl.Http;

using PassGuardia.Contracts.Http;
using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Commands;
using PassGuardia.Domain.Constants;
using PassGuardia.Domain.Queries;

namespace PassGuardia.IntegrationTest;

public class AuditTests : Base
{
    public AuditTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("HelloWorld")]
    public async Task AuditLogShouldGetOk(string password)
    {
        var createdPassword = await CreateUserAndUsersPassword(password, Roles.User, Visibility.Public);

        var admin = await CreateTestRole(Roles.Admin);

        await _apiClient.GetPassword(createdPassword.PasswordId.ToString());

        var audit = await GetAudit(admin.Token);

        AuditShouldBe(audit, HttpStatusCode.OK, $"/passwords/{createdPassword.PasswordId}");
    }

    [Theory]
    [InlineData("invalid-guid-format")]
    public async Task AuditLogGetShouldReturnBadRequest(string id)
    {
        try
        {
            var admin = await CreateTestRole(Roles.Admin);

            await _apiClient.GetPassword(id);

            var audit = await GetAudit(admin.Token);

            Assert.Fail("Should have thrown FlurtException");
        }
        catch (FlurlHttpException ex)
        {
            ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

            var result = await ex.GetResponseJsonAsync<ErrorResponse>();

            result.Errors.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData("details")]
    [InlineData(" ")]
    public async Task CreatePasswordUserAuditShouldReturnExceptionLog(string password)
    {
        var admin = await CreateTestRole(Roles.Admin);

        await CreateUserAndUsersPassword(password, Roles.User, Visibility.Public);

        var audit = await GetAudit(admin.Token);

        AuditShouldBe(audit, HttpStatusCode.Created, "/passwords", "POST");
    }

    [Fact]
    public async Task RegisterAndAuthenticateUserShouldGetOkAuditLog()
    {
        var admin = await CreateTestRole(Roles.Admin);

        var registerRequest = new RegisterUserRequest
        {
            Username = $"test_{_faker.Database.Random.Uuid()}".Replace("-", "").Substring(0, 15),
            Password = $"Aa1!_{_faker.Internet.Password()}"
        };
        var registerResponse = await _apiClient.RegisterUser(registerRequest);

        var registerAudit = await GetAudit(admin.Token);

        AuditShouldBe(registerAudit, HttpStatusCode.Created, "/users", "POST");

        registerResponse.UserId.Should().NotBeNullOrEmpty();

        var authenticateResponse = await _apiClient.AuthenticateUser(new AuthenticateUserRequest
        {
            Username = registerRequest.Username,
            Password = registerRequest.Password
        });

        var authenticateAudit = await GetAudit(admin.Token);

        AuditShouldBe(authenticateAudit, HttpStatusCode.OK, "/users/authenticate", "POST");

        authenticateResponse.Should().NotBeNull();
        authenticateResponse.Token.Should().NotBeNullOrEmpty();

        var meResponse = await _apiClient.Me(authenticateResponse.Token);

        var meAudit = await GetAudit(admin.Token);

        AuditShouldBe(meAudit, HttpStatusCode.OK, "/users/me");

        meResponse.Should().NotBeNull();
        meResponse.Username.Should().Be(registerRequest.Username);
        meResponse.Roles.Should().BeEquivalentTo([Roles.User]);
    }

    private async Task<CreatePasswordResponse> CreateUserAndUsersPassword(string password, string role = Roles.User, Visibility visibility = Visibility.Private)
    {
        var user = await CreateTestRole(role);

        return await _apiClient.CreatePassword(new PasswordRequest
        {
            Password = password,
            Visibility = visibility
        }, user.Token);
    }

    private async Task<Audit> GetAudit(string token, int pageNumber = 1, int pageSize = 1)
    {
        var propAudit = new GetAuditLogQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var audits = await _apiClient.GetAudit(propAudit, token);

        return audits.Audits.FirstOrDefault();
    }

    private void AuditShouldBe(Audit audit, HttpStatusCode httpStatusCode, string requestPath, string requestMethod = "GET")
    {
        var currentLocalTime = DateTime.Now;
        var auditLocalTime = TimeZoneInfo.ConvertTimeFromUtc(audit.Timestamp, TimeZoneInfo.Local);

        audit.StatusCode.Should().Be((int)httpStatusCode);
        audit.RequestPath.Should().Be(requestPath);
        auditLocalTime.Should().BeCloseTo(currentLocalTime, TimeSpan.FromSeconds(5));
        audit.RequestMethod.Should().Be(requestMethod);
        audit.Exception.Should().BeNull();
    }
}