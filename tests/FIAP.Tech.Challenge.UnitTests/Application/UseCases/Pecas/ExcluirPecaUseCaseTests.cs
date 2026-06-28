using FIAP.Tech.Challenge.Application.UseCases.Pecas;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Pecas;

public class ExcluirPecaUseCaseTests
{
    private readonly IPecaRepository _pecaRepositoryMock;
    private readonly IOrdemServicoRepository _ordemServicoRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ExcluirPecaUseCase _useCase;

    public ExcluirPecaUseCaseTests()
    {
        _pecaRepositoryMock = Substitute.For<IPecaRepository>();
        _ordemServicoRepositoryMock = Substitute.For<IOrdemServicoRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new ExcluirPecaUseCase(_pecaRepositoryMock, _ordemServicoRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_PecaInexistente_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _pecaRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Peca)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(id);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Peça não encontrada no catálogo.");

        _pecaRepositoryMock.DidNotReceive().Remover(Arg.Any<Peca>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_PecaComOrdemServico_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var peca = new Peca(id, "Pastilha", 100.00m, 5);

        _pecaRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(peca);

        var ordens = new List<OrdemServico>
        {
            new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Barulho")
        };
        // Adicionar item com a peça
        ordens[0].AdicionarItem(id, "Pastilha", 1, 100.00m, 0);

        _ordemServicoRepositoryMock.ObterTodasAsync(Arg.Any<CancellationToken>())
            .Returns(ordens);

        // Act
        var act = () => _useCase.ExecutarAsync(id);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Não é possível excluir uma peça associada a ordens de serviço.");

        _pecaRepositoryMock.DidNotReceive().Remover(Arg.Any<Peca>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_SemVinculos_DeveRemoverECommitar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var peca = new Peca(id, "Pastilha", 100.00m, 5);

        _pecaRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(peca);

        _ordemServicoRepositoryMock.ObterTodasAsync(Arg.Any<CancellationToken>())
            .Returns(new List<OrdemServico>());

        // Act
        await _useCase.ExecutarAsync(id, TestContext.Current.CancellationToken);

        // Assert
        _pecaRepositoryMock.Received(1).Remover(peca);
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
