using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Clientes;

public class AtualizarVeiculoUseCaseTests
{
    private readonly IVeiculoRepository _veiculoRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly AtualizarVeiculoUseCase _useCase;

    public AtualizarVeiculoUseCaseTests()
    {
        _veiculoRepositoryMock = Substitute.For<IVeiculoRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new AtualizarVeiculoUseCase(_veiculoRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_VeiculoInexistente_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarVeiculoRequest
        {
            Placa = "XYZ9D87",
            Marca = "Ford",
            Modelo = "Ka",
            Ano = 2020
        };

        _veiculoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Veiculo)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Veículo não encontrado.");

        await _veiculoRepositoryMock.DidNotReceive().AtualizarAsync(Arg.Any<Veiculo>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_PlacaCadastradaEmOutroVeiculo_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarVeiculoRequest
        {
            Placa = "XYZ9D87",
            Marca = "Ford",
            Modelo = "Ka",
            Ano = 2020
        };

        var veiculo = new Veiculo(id, new Placa("ABC1D23"), "Ford", "Ka", 2020, Guid.NewGuid());
        var veiculoComMesmaPlaca = new Veiculo(Guid.NewGuid(), new Placa("XYZ9D87"), "Fiat", "Uno", 2015, Guid.NewGuid());

        _veiculoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(veiculo);

        _veiculoRepositoryMock.ObterPorPlacaAsync(Arg.Any<Placa>(), Arg.Any<CancellationToken>())
            .Returns(veiculoComMesmaPlaca);

        // Act
        var act = () => _useCase.ExecutarAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Outro veículo já está cadastrado com esta placa.");

        await _veiculoRepositoryMock.DidNotReceive().AtualizarAsync(Arg.Any<Veiculo>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_VeiculoExistente_DeveAtualizarECommitar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarVeiculoRequest
        {
            Placa = "XYZ9D87",
            Marca = "Ford",
            Modelo = "Ka",
            Ano = 2020
        };

        var veiculo = new Veiculo(id, new Placa("ABC1D23"), "Ford", "Ka", 2019, Guid.NewGuid());

        _veiculoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(veiculo);

        _veiculoRepositoryMock.ObterPorPlacaAsync(Arg.Any<Placa>(), Arg.Any<CancellationToken>())
            .Returns((Veiculo)null!);

        // Act
        var response = await _useCase.ExecutarAsync(id, request, TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Placa.Should().Be("XYZ9D87");
        response.Marca.Should().Be("Ford");
        response.Modelo.Should().Be("Ka");
        response.Ano.Should().Be(2020);

        await _veiculoRepositoryMock.Received(1).AtualizarAsync(veiculo, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
