using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Clientes;

public class CriarClienteUseCaseTests
{
    private readonly IClienteRepository _clienteRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly CriarClienteUseCase _useCase;

    public CriarClienteUseCaseTests()
    {
        _clienteRepositoryMock = Substitute.For<IClienteRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new CriarClienteUseCase(_clienteRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_CpfJaCadastrado_DeveLancarDominioException()
    {
        // Arrange
        var request = new CriarClienteRequest
        {
            Nome = "Cliente Existente",
            Cpf = "12345678909",
            Email = "teste@email.com",
            Telefone = "11999999999"
        };

        var clienteExistente = new Cliente(Guid.NewGuid(), "Outro Nome", new Cpf("12345678909"), "outro@email.com",
            "11988888888");

        _clienteRepositoryMock.ObterPorCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>())
            .Returns(clienteExistente);

        // Act
        var act = () => _useCase.ExecutarAsync(request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Cliente com este CPF já está cadastrado.");

        await _clienteRepositoryMock.DidNotReceive().AdicionarAsync(Arg.Any<Cliente>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_NovoCliente_DeveCriarECommitar()
    {
        // Arrange
        var request = new CriarClienteRequest
        {
            Nome = "Novo Cliente",
            Cpf = "12345678909",
            Email = "novo@email.com",
            Telefone = "11999999999"
        };

        _clienteRepositoryMock.ObterPorCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>())
            .Returns((Cliente)null!);

        // Act
        var response = await _useCase.ExecutarAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Nome.Should().Be("Novo Cliente");
        response.Cpf.Should().Be("12345678909");
        response.Email.Should().Be("novo@email.com");
        response.Telefone.Should().Be("11999999999");

        await _clienteRepositoryMock.Received(1).AdicionarAsync(Arg.Any<Cliente>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}