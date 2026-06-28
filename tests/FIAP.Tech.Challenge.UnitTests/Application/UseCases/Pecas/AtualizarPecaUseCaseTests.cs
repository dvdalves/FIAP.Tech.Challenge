using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.UseCases.Pecas;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Pecas;

public class AtualizarPecaUseCaseTests
{
    private readonly IPecaRepository _pecaRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly AtualizarPecaUseCase _useCase;

    public AtualizarPecaUseCaseTests()
    {
        _pecaRepositoryMock = Substitute.For<IPecaRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new AtualizarPecaUseCase(_pecaRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_PecaInexistente_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarPecaRequest
        {
            Nome = "Novo Nome",
            Preco = 180.00m
        };

        _pecaRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Peca)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Peça não encontrada no catálogo.");

        await _pecaRepositoryMock.DidNotReceive().AtualizarAsync(Arg.Any<Peca>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_PecaExistente_DeveAtualizarECommitar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarPecaRequest
        {
            Nome = "Nome Atualizado",
            Preco = 180.00m
        };

        var peca = new Peca(id, "Nome Antigo", 150.00m, 5);

        _pecaRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(peca);

        // Act
        var response = await _useCase.ExecutarAsync(id, request, TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Nome.Should().Be("Nome Atualizado");
        response.Preco.Should().Be(180.00m);
        response.QuantidadeEstoque.Should().Be(5);

        await _pecaRepositoryMock.Received(1).AtualizarAsync(peca, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
