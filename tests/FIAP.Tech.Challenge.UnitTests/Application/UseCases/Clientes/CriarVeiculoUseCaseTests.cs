using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using NSubstitute;
using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Clientes;

public class CriarVeiculoUseCaseTests
{
    private readonly IVeiculoRepository _veiculoRepositoryMock;
    private readonly IClienteRepository _clienteRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly CriarVeiculoUseCase _useCase;

    public CriarVeiculoUseCaseTests()
    {
        _veiculoRepositoryMock = Substitute.For<IVeiculoRepository>();
        _clienteRepositoryMock = Substitute.For<IClienteRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new CriarVeiculoUseCase(_veiculoRepositoryMock, _clienteRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_ClienteNaoEncontrado_DeveLancarDominioException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var request = new CriarVeiculoRequest { Placa = "AAA-1234", Marca = "Ford", Modelo = "Ka", Ano = 2018 };

        _clienteRepositoryMock.ObterPorIdAsync(clienteId, Arg.Any<CancellationToken>())
            .Returns((Cliente)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(clienteId, request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Cliente não encontrado.");

        await _veiculoRepositoryMock.DidNotReceive().AdicionarAsync(Arg.Any<Veiculo>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_PlacaExistenteParaMesmoCliente_DeveLancarDominioException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var request = new CriarVeiculoRequest { Placa = "AAA-1234", Marca = "Ford", Modelo = "Ka", Ano = 2018 };

        var cliente = new Cliente(clienteId, "Nome", new Cpf("12345678909"), "email@email.com", "11999999999");
        var veiculoExistente = new Veiculo(Guid.NewGuid(), new Placa("AAA-1234"), "Ford", "Ka", 2018, clienteId);

        _clienteRepositoryMock.ObterPorIdAsync(clienteId, Arg.Any<CancellationToken>())
            .Returns(cliente);
        _veiculoRepositoryMock.ObterPorPlacaAsync(Arg.Any<Placa>(), Arg.Any<CancellationToken>())
            .Returns(veiculoExistente);

        // Act
        var act = () => _useCase.ExecutarAsync(clienteId, request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Este veículo já está cadastrado para este cliente.");
    }

    [Fact]
    public async Task ExecutarAsync_PlacaExistenteParaOutroCliente_DeveLancarDominioException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var outroClienteId = Guid.NewGuid();
        var request = new CriarVeiculoRequest { Placa = "AAA-1234", Marca = "Ford", Modelo = "Ka", Ano = 2018 };

        var cliente = new Cliente(clienteId, "Nome", new Cpf("12345678909"), "email@email.com", "11999999999");
        var veiculoExistente = new Veiculo(Guid.NewGuid(), new Placa("AAA-1234"), "Ford", "Ka", 2018, outroClienteId);

        _clienteRepositoryMock.ObterPorIdAsync(clienteId, Arg.Any<CancellationToken>())
            .Returns(cliente);
        _veiculoRepositoryMock.ObterPorPlacaAsync(Arg.Any<Placa>(), Arg.Any<CancellationToken>())
            .Returns(veiculoExistente);

        // Act
        var act = () => _useCase.ExecutarAsync(clienteId, request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Este veículo já está cadastrado para outro cliente.");
    }

    [Fact]
    public async Task ExecutarAsync_NovoVeiculo_DeveCriarECommitar()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var request = new CriarVeiculoRequest { Placa = "AAA-1234", Marca = "Ford", Modelo = "Ka", Ano = 2018 };

        var cliente = new Cliente(clienteId, "Nome", new Cpf("12345678909"), "email@email.com", "11999999999");

        _clienteRepositoryMock.ObterPorIdAsync(clienteId, Arg.Any<CancellationToken>())
            .Returns(cliente);
        _veiculoRepositoryMock.ObterPorPlacaAsync(Arg.Any<Placa>(), Arg.Any<CancellationToken>())
            .Returns((Veiculo)null!);

        // Act
        var response = await _useCase.ExecutarAsync(clienteId, request);

        // Assert
        response.Should().NotBeNull();
        response.Placa.Should().Be("AAA1234");
        response.Marca.Should().Be("Ford");
        response.Modelo.Should().Be("Ka");
        response.Ano.Should().Be(2018);
        response.ClienteId.Should().Be(clienteId);

        await _veiculoRepositoryMock.Received(1).AdicionarAsync(Arg.Any<Veiculo>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
