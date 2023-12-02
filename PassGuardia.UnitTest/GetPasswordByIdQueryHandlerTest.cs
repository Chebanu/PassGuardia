using System;
using System.Text;

using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Options;

using Moq;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Algorithm;
using PassGuardia.Domain.Configuration;
using PassGuardia.Domain.Queries;
using PassGuardia.Domain.Repositories;

namespace PassGuardia.UnitTest;

public class GetPasswordByIdQueryHandlerTest
{
    private readonly Mock<IRepository> _repositoryMock = new();
    private readonly Mock<IEncryptor> _encryptorMock = new();
    private readonly Mock<IOptionsMonitor<PassGuardiaConfig>> _optionMock = new();

    private readonly IRequestHandler<GetPasswordByIdQuery, GetPasswordByIdResult> _handler;

    public GetPasswordByIdQueryHandlerTest()
    {
        _handler = new GetPasswordByIdQueryHandler(_repositoryMock.Object, _encryptorMock.Object, _optionMock.Object);
    }

    #region GetPasswordTests
    [Fact]
    public async Task HandleShouldReturnEncryptedPassword()
    {
        //Arrange
        var id = Guid.NewGuid();
        var encryptedPassword = Encoding.UTF8.GetBytes("HelloWorld");


        var config = new PassGuardiaConfig { EncryptionKey = Encoding.UTF8.GetBytes("IsItWorking") };
        var query = new GetPasswordByIdQuery { Id = id };
        var dbPassword = new Password { Id = id, EncryptedPassword = encryptedPassword };

        _ = _optionMock.Setup(x => x.CurrentValue).Returns(config);
        _ = _repositoryMock.Setup(x => x.GetPasswordById(id, It.IsAny<CancellationToken>())).ReturnsAsync(dbPassword);
        _ = _encryptorMock.Setup(x => x.Decrypt(encryptedPassword, config.EncryptionKey)).Returns("decryptedPassword");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Password.Should().Be("decryptedPassword");
    }

    [Fact]
    public async Task HandleShouldGetNotFoundError()
    {
        var id = Guid.NewGuid();
        var encryptedPassword = Encoding.UTF8.GetBytes("HelloWorld");

        var config = new PassGuardiaConfig { EncryptionKey = Encoding.UTF8.GetBytes("IsItWorking") };
        var query = new GetPasswordByIdQuery { Id = Guid.NewGuid() };
        var dbPassword = new Password { Id = id, EncryptedPassword = encryptedPassword };

        _ = _optionMock.Setup(x => x.CurrentValue).Returns(config);
        _ = _repositoryMock.Setup(x => x.GetPasswordById(Guid.NewGuid(), It.IsAny<CancellationToken>())).ReturnsAsync(dbPassword);
        _ = _encryptorMock.Setup(x => x.Decrypt(encryptedPassword, config.EncryptionKey)).Returns("decryptedPassword");

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NullReferenceException>();
    }
    #endregion
}