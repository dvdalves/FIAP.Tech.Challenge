using FIAP.Tech.Challenge.Application.UseCases.Servicos;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Servicos;

public class AtualizarServicoUseCaseTests
{
    private readonly IServicoRepository _servicoRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly AtualizarServicoUseCase _useCase;

    public AtualizarServicoUseCaseTests()
    {
        _servicoRepositoryMock = Substitute.For<IServicoRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new AtualizarServicoUseCase(_servicoRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_ServicoInexistente_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarServicoRequest { Nome = "Alinhamento Novo", PrecoMaoDeObra = 150.00m };

        _servicoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Servico)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Serviço não encontrado.");

        await _servicoRepositoryMock.DidNotReceive().AtualizarAsync(Arg.Any<Servico>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_ServicoExistente_DeveAtualizarECommitar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new AtualizarServicoRequest { Nome = "Alinhamento Novo", PrecoMaoDeObra = 150.00m };

        var servico = new Servico(id, "Alinhamento Antigo", 120.00m);

        _servicoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(servico);

        // Act
        var response = await _useCase.ExecutarAsync(id, request, TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Nome.Should().Be("Alinhamento Novo");
        response.PrecoMaoDeObra.Should().Be(150.00m);

        await _servicoRepositoryMock.Received(1).AtualizarAsync(servico, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
