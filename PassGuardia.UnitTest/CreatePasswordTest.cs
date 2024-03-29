﻿using System.Text;

using FluentAssertions;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
    private readonly Mock<UserManager<IdentityUser>> _userMock = new();

    private readonly IRequestHandler<CreatePasswordCommand, CreatePasswordResult> _handlerCreator;

    public CreatePasswordTest()
    {
        _userMock = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(),
                                                        Mock.Of<IOptions<IdentityOptions>>(),
                                                        Mock.Of<IPasswordHasher<IdentityUser>>(),
                                                        Array.Empty<IUserValidator<IdentityUser>>(),
                                                        Array.Empty<IPasswordValidator<IdentityUser>>(),
                                                        Mock.Of<ILookupNormalizer>(),
                                                        Mock.Of<IdentityErrorDescriber>(),
                                                        Mock.Of<IServiceProvider>(),
                                                        Mock.Of<ILogger<UserManager<IdentityUser>>>());

        _handlerCreator = new CreatePasswordCommandHandler(_repositoryMock.Object,
                                                        _encryptorMock.Object,
                                                        _optionMock.Object,
                                                        _userMock.Object,
                                                        Mock.Of<ILogger<CreatePasswordCommandHandler>>());
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
    #endregion
}