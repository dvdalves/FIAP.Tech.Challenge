using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.UseCases.Pecas;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Pecas;

public class CriarPecaUseCaseTests
{
    private readonly IPecaRepository _pecaRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly CriarPecaUseCase _useCase;

    public CriarPecaUseCaseTests()
    {
        _pecaRepositoryMock = Substitute.For<IPecaRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new CriarPecaUseCase(_pecaRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_DadosValidos_DeveCriarECommitar()
    {
        // Arrange
        var request = new AdicionarPecaRequest
        {
            Nome = "Pastilha de freio",
            Preco = 150.00m,
            QuantidadeEstoque = 10
        };

        // Act
        var response = await _useCase.ExecutarAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Nome.Should().Be("Pastilha de freio");
        response.Preco.Should().Be(150.00m);
        response.QuantidadeEstoque.Should().Be(10);

        await _pecaRepositoryMock.Received(1).AdicionarAsync(Arg.Any<Peca>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
