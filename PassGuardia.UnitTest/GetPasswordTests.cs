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

public class GetPasswordTests
{
    private readonly Mock<IPasswordRepository> _repositoryMock = new();
    private readonly Mock<IEncryptor> _encryptorMock = new();
    private readonly Mock<IOptionsMonitor<PassGuardiaConfig>> _optionMock = new();

    private readonly IRequestHandler<GetPasswordByIdQuery, GetPasswordByIdResult> _handlerGetter;

    public GetPasswordTests()
    {
        _handlerGetter = new GetPasswordByIdQueryHandler(_repositoryMock.Object, _encryptorMock.Object, _optionMock.Object);
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
        var result = await _handlerGetter.Handle(query, CancellationToken.None);

        // Assert
        _ = result.Should().NotBeNull();
        _ = result.Password.Should().Be("decryptedPassword");

        _optionMock.Verify(x => x.CurrentValue, Times.Once);
        _repositoryMock.Verify(x => x.GetPasswordById(id, It.IsAny<CancellationToken>()), Times.Once);
        _encryptorMock.Verify(x => x.Decrypt(encryptedPassword, config.EncryptionKey), Times.Once);
    }

    [Fact]
    public async Task HandleShouldGetNotFoundError()
    {
        //Arrange
        var id = Guid.NewGuid();
        var encryptedPassword = Encoding.UTF8.GetBytes("HelloWorld");

        var config = new PassGuardiaConfig { EncryptionKey = Encoding.UTF8.GetBytes("DoesItWork") };
        var dbPassword = new Password { Id = id, EncryptedPassword = encryptedPassword };

        var query = new GetPasswordByIdQuery { Id = Guid.NewGuid() };

        _ = _optionMock.Setup(x => x.CurrentValue).Returns(config);
        _ = _repositoryMock.Setup(x => x.GetPasswordById(Guid.NewGuid(), It.IsAny<CancellationToken>())).ReturnsAsync(dbPassword);
        _ = _encryptorMock.Setup(x => x.Decrypt(encryptedPassword, config.EncryptionKey)).Returns("decryptedPassword");

        //Act
        var result = await _handlerGetter.Handle(query, CancellationToken.None);

        //Assert
        _ = result.Password.Should().BeNull();
    }
    #endregion


}