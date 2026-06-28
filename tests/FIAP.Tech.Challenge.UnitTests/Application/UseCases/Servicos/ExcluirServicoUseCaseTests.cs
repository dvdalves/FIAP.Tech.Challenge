using FIAP.Tech.Challenge.Application.UseCases.Servicos;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Servicos;

public class ExcluirServicoUseCaseTests
{
    private readonly IServicoRepository _servicoRepositoryMock;
    private readonly IOrdemServicoRepository _ordemServicoRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ExcluirServicoUseCase _useCase;

    public ExcluirServicoUseCaseTests()
    {
        _servicoRepositoryMock = Substitute.For<IServicoRepository>();
        _ordemServicoRepositoryMock = Substitute.For<IOrdemServicoRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new ExcluirServicoUseCase(_servicoRepositoryMock, _ordemServicoRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_ServicoInexistente_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _servicoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Servico)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(id);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Serviço não encontrado.");

        _servicoRepositoryMock.DidNotReceive().Remover(Arg.Any<Servico>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_ServicoComOrdemServico_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var servico = new Servico(id, "Alinhamento", 120.00m);

        _servicoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(servico);

        var ordens = new List<OrdemServico>
        {
            new OrdemServico(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Barulho")
        };
        // Adicionar item de serviço com descrição correspondente
        ordens[0].AdicionarItem(null, "Alinhamento", 1, 0, 120.00m);

        _ordemServicoRepositoryMock.ObterTodasAsync(Arg.Any<CancellationToken>())
            .Returns(ordens);

        // Act
        var act = () => _useCase.ExecutarAsync(id);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Não é possível excluir um serviço associado a ordens de serviço.");

        _servicoRepositoryMock.DidNotReceive().Remover(Arg.Any<Servico>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_SemVinculos_DeveRemoverECommitar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var servico = new Servico(id, "Alinhamento", 120.00m);

        _servicoRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(servico);

        _ordemServicoRepositoryMock.ObterTodasAsync(Arg.Any<CancellationToken>())
            .Returns(new List<OrdemServico>());

        // Act
        await _useCase.ExecutarAsync(id, TestContext.Current.CancellationToken);

        // Assert
        _servicoRepositoryMock.Received(1).Remover(servico);
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
