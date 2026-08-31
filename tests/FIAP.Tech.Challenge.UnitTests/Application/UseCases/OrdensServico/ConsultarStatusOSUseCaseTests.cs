using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.OrdensServico;

public class ConsultarStatusOSUseCaseTests
{
    private readonly IOrdemServicoRepository _ordemServicoRepositoryMock;
    private readonly ConsultarStatusOSUseCase _useCase;

    public ConsultarStatusOSUseCaseTests()
    {
        _ordemServicoRepositoryMock = Substitute.For<IOrdemServicoRepository>();
        _useCase = new ConsultarStatusOSUseCase(_ordemServicoRepositoryMock);
    }

    [Fact]
    public async Task ExecutarAsync_ComOSExistente_DeveRetornarStatusDetalhamento()
    {
        // Arrange
        var osId = Guid.NewGuid();
        var os = new OrdemServico(osId, Guid.NewGuid(), Guid.NewGuid(), "Troca de amortecedor");

        _ordemServicoRepositoryMock.ObterPorIdAsync(osId, Arg.Any<CancellationToken>())
            .Returns(os);

        // Act
        var response = await _useCase.ExecutarAsync(osId);

        // Assert
        response.Should().NotBeNull();
        response.OrdemServicoId.Should().Be(osId);
        response.Status.Should().Be("Recebida");
        response.DescricaoStatus.Should().Contain("Recebida");
    }

    [Fact]
    public async Task ExecutarAsync_ComOSInexistente_DeveLancarDominioException()
    {
        // Arrange
        var osId = Guid.NewGuid();
        _ordemServicoRepositoryMock.ObterPorIdAsync(osId, Arg.Any<CancellationToken>())
            .Returns((OrdemServico)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(osId);

        // Assert
        await act.Should().ThrowAsync<DominioException>().WithMessage("*não encontrada*");
    }
}
