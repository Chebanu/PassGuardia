using System.Text;

using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Options;

using Moq;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Algorithm;
using PassGuardia.Domain.Commands;
using PassGuardia.Domain.Configuration;
using PassGuardia.Domain.Repositories;

namespace PassGuardia.UnitTest;

public class CreatePasswordTest
{
    private readonly Mock<IPasswordRepository> _repositoryMock = new();
    private readonly Mock<IEncryptor> _encryptorMock = new();
    private readonly Mock<IOptionsMonitor<PassGuardiaConfig>> _optionMock = new();

    private readonly IRequestHandler<CreatePasswordCommand, CreatePasswordResult> _handlerCreator;

    public CreatePasswordTest()
    {
        _handlerCreator = new CreatePasswordCommandHandler(_repositoryMock.Object, _encryptorMock.Object, _optionMock.Object);
    }

    #region CreatePasswordTests
    [Fact]
    public async Task CreatePasswordCommandHandler_ShouldCreatePassword()
    {
        // Arrange
        var id = Guid.NewGuid();
        var plainTextPassword = "HelloWorld";

        var config = new PassGuardiaConfig { EncryptionKey = Encoding.UTF8.GetBytes("DoesItWork") };
        var command = new CreatePasswordCommand { Password = plainTextPassword };
        var encryptedPassWordBytes = Encoding.ASCII.GetBytes("EncryptedPassword");

        var createdPassword = new Password { Id = id, EncryptedPassword = encryptedPassWordBytes };

        _ = _optionMock.Setup(x => x.CurrentValue).Returns(config);
        _ = _encryptorMock.Setup(x => x.Encrypt(plainTextPassword, config.EncryptionKey)).Returns(encryptedPassWordBytes);
        _ = _repositoryMock.Setup(x => x.CreatePassword(It.IsAny<Password>(), It.IsAny<CancellationToken>())).ReturnsAsync(createdPassword);

        //Act
        var result = await _handlerCreator.Handle(command, CancellationToken.None);

        //Assert
        _ = result.Should().NotBeNull();

        _optionMock.Verify(x => x.CurrentValue, Times.Once);
        _encryptorMock.Verify(x => x.Encrypt(plainTextPassword, config.EncryptionKey), Times.Once);
        _repositoryMock.Verify(x => x.CreatePassword(It.IsAny<Password>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("dzPVGfry7Qfdbri3bvz73ro0VGj5d4GcUC5NMOsRx6hOUtDbXq4qrmap41BXN9h6gG6TuvgKcV5MdeACABQ1MYx8BQnLNaX1Me7cXKlBu8VQex3dwQO0HpPBClHlEHUyNegLQOoQbFkgX1X2c8rwozu2jqWkw5peTEmfHdMs6BOZKpVmS5Pg1nZ5rgB3v8S2AOcn9eHQBJ8d5A5RnphrT9azfoUJyAUERgVzC99lK3HBXApPa8ugj1q54BIeuggLu2c2")]
    public async Task CreatePassword_ArgumentException_EmptyString(string invalidPassword)
    {
        var command = new CreatePasswordCommand { Password = invalidPassword };

        // Act
        var act = async () => await _handlerCreator.Handle(command, CancellationToken.None);

        //Assert
        _ = await act.Should().ThrowAsync<ArgumentException>();
    }
    #endregion
}