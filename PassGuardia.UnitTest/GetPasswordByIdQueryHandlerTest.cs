using FluentAssertions;
using MediatR;
using Moq;
using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Queries;
using PassGuardia.Domain.Repositories;
using System.Text;

namespace PassGuardia.UnitTest;

public class GetPasswordByIdQueryHandlerTest
{
    private readonly Mock<IRepository> _repositoryMock = new();

    private readonly IRequestHandler<GetPasswordByIdQuery, GetPasswordByIdResult> _handler;

    public GetPasswordByIdQueryHandlerTest()
    {
        _handler = new GetPasswordByIdQueryHandler(
            _repositoryMock.Object
            );
    }

    [Fact]
    public async Task GetPasswordByIdQueryHandler_ShouldReturnCorrectPassword()
    {
        //doesnt work. to be continue...
    }
}
