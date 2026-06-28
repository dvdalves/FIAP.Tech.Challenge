using FIAP.Tech.Challenge.Application.UseCases.Pecas;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Pecas;

public class AjustarEstoquePecaUseCaseTests
{
    private readonly IPecaRepository _pecaRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly AjustarEstoquePecaUseCase _useCase;

    public AjustarEstoquePecaUseCaseTests()
    {
        _pecaRepositoryMock = Substitute.For<IPecaRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new AjustarEstoquePecaUseCase(_pecaRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_PecaNaoEncontrada_DeveLancarDominioException()
    {
        // Arrange
        var pecaId = Guid.NewGuid();
        _pecaRepositoryMock.ObterPorIdAsync(pecaId, Arg.Any<CancellationToken>())
            .Returns((Peca)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(pecaId, 20);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Peça não encontrada no estoque.");

        await _pecaRepositoryMock.DidNotReceive().AtualizarAsync(Arg.Any<Peca>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_PecaEncontrada_DeveAjustarEstoqueECommitar()
    {
        // Arrange
        var pecaId = Guid.NewGuid();
        var peca = new Peca(pecaId, "Filtro de Ar", 35.00m, 5);
        _pecaRepositoryMock.ObterPorIdAsync(pecaId, Arg.Any<CancellationToken>())
            .Returns(peca);

        // Act
        await _useCase.ExecutarAsync(pecaId, 15, TestContext.Current.CancellationToken);

        // Assert
        peca.QuantidadeEstoque.Should().Be(15);
        await _pecaRepositoryMock.Received(1).AtualizarAsync(peca, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}