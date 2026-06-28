using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Clientes;

public class AtualizarClienteUseCaseTests
{
    private readonly IClienteRepository _clienteRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly AtualizarClienteUseCase _useCase;

    public AtualizarClienteUseCaseTests()
    {
        _clienteRepositoryMock = Substitute.For<IClienteRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new AtualizarClienteUseCase(_clienteRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_ClienteInexistente_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarClienteRequest
        {
            Nome = "Cliente Atualizado",
            Email = "atualizado@email.com",
            Telefone = "11988887777"
        };

        _clienteRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Cliente)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Cliente não encontrado.");

        await _clienteRepositoryMock.DidNotReceive().AtualizarAsync(Arg.Any<Cliente>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_ClienteExistente_DeveAtualizarECommitar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarClienteRequest
        {
            Nome = "Nome Atualizado",
            Email = "atualizado@email.com",
            Telefone = "11988887777"
        };

        var cliente = new Cliente(id, "Nome Antigo", new Cpf("12345678909"), "antigo@email.com", "11999999999");

        _clienteRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(cliente);

        // Act
        var response = await _useCase.ExecutarAsync(id, request, TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Nome.Should().Be("Nome Atualizado");
        response.Email.Should().Be("atualizado@email.com");
        response.Telefone.Should().Be("11988887777");

        await _clienteRepositoryMock.Received(1).AtualizarAsync(cliente, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
