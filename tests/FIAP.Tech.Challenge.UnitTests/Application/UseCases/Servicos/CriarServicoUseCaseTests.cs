using FIAP.Tech.Challenge.Application.UseCases.Servicos;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Servicos;

public class CriarServicoUseCaseTests
{
    private readonly IServicoRepository _servicoRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly CriarServicoUseCase _useCase;

    public CriarServicoUseCaseTests()
    {
        _servicoRepositoryMock = Substitute.For<IServicoRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new CriarServicoUseCase(_servicoRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_NomeVazio_DeveLancarDominioException()
    {
        // Arrange
        var request = new CriarServicoRequest { Nome = "", PrecoMaoDeObra = 100.00m };

        // Act
        var act = () => _useCase.ExecutarAsync(request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Nome do serviço é obrigatório.");
    }

    [Fact]
    public async Task ExecutarAsync_PrecoNegativo_DeveLancarDominioException()
    {
        // Arrange
        var request = new CriarServicoRequest { Nome = "Alinhamento", PrecoMaoDeObra = -50.00m };

        // Act
        var act = () => _useCase.ExecutarAsync(request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("O preço do serviço não pode ser negativo.");
    }

    [Fact]
    public async Task ExecutarAsync_DadosValidos_DeveCriarECommitar()
    {
        // Arrange
        var request = new CriarServicoRequest { Nome = "Alinhamento", PrecoMaoDeObra = 120.00m };

        // Act
        var response = await _useCase.ExecutarAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Nome.Should().Be("Alinhamento");
        response.PrecoMaoDeObra.Should().Be(120.00m);

        await _servicoRepositoryMock.Received(1).AdicionarAsync(Arg.Any<Servico>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
