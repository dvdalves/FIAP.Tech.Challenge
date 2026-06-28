using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Clientes;

public class ExcluirVeiculoUseCaseTests
{
    private readonly IVeiculoRepository _veiculoRepositoryMock;
    private readonly IOrdemServicoRepository _ordemServicoRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ExcluirVeiculoUseCase _useCase;

    public ExcluirVeiculoUseCaseTests()
    {
        _veiculoRepositoryMock = Substitute.For<IVeiculoRepository>();
        _ordemServicoRepositoryMock = Substitute.For<IOrdemServicoRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new ExcluirVeiculoUseCase(_veiculoRepositoryMock, _ordemServicoRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_VeiculoInexistente_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _veiculoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Veiculo)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(id);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Veículo não encontrado.");

        _veiculoRepositoryMock.DidNotReceive().Remover(Arg.Any<Veiculo>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_VeiculoComOrdemServico_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var veiculo = new Veiculo(id, new Placa("ABC1D23"), "Ford", "Ka", 2020, Guid.NewGuid());

        _veiculoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(veiculo);

        var ordens = new List<OrdemServico>
        {
            new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), id, "Troca de óleo")
        };

        _ordemServicoRepositoryMock.ObterTodasAsync(Arg.Any<CancellationToken>())
            .Returns(ordens);

        // Act
        var act = () => _useCase.ExecutarAsync(id);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Não é possível excluir um veículo com ordens de serviço vinculadas.");

        _veiculoRepositoryMock.DidNotReceive().Remover(Arg.Any<Veiculo>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_SemVinculos_DeveRemoverECommitar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var veiculo = new Veiculo(id, new Placa("ABC1D23"), "Ford", "Ka", 2020, Guid.NewGuid());

        _veiculoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(veiculo);

        _ordemServicoRepositoryMock.ObterTodasAsync(Arg.Any<CancellationToken>())
            .Returns(new List<OrdemServico>());

        // Act
        await _useCase.ExecutarAsync(id, TestContext.Current.CancellationToken);

        // Assert
        _veiculoRepositoryMock.Received(1).Remover(veiculo);
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
