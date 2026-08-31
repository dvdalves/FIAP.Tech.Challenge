using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Services;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.OrdensServico;

public class ProcessarNotificacaoOrcamentoUseCaseTests
{
    private readonly IOrdemServicoRepository _ordemServicoRepositoryMock;
    private readonly IPecaRepository _pecaRepositoryMock;
    private readonly IClienteRepository _clienteRepositoryMock;
    private readonly IServicoNotificacao _servicoNotificacaoMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ProcessarNotificacaoOrcamentoUseCase _useCase;

    public ProcessarNotificacaoOrcamentoUseCaseTests()
    {
        _ordemServicoRepositoryMock = Substitute.For<IOrdemServicoRepository>();
        _pecaRepositoryMock = Substitute.For<IPecaRepository>();
        _clienteRepositoryMock = Substitute.For<IClienteRepository>();
        _servicoNotificacaoMock = Substitute.For<IServicoNotificacao>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        var aprovarUseCase = new AprovarOrcamentoUseCase(
            _ordemServicoRepositoryMock,
            _pecaRepositoryMock,
            _clienteRepositoryMock,
            _servicoNotificacaoMock,
            _unitOfWorkMock);

        var rejeitarUseCase = new RejeitarOrcamentoUseCase(
            _ordemServicoRepositoryMock,
            _clienteRepositoryMock,
            _servicoNotificacaoMock,
            _unitOfWorkMock);

        _useCase = new ProcessarNotificacaoOrcamentoUseCase(aprovarUseCase, rejeitarUseCase);
    }

    [Fact]
    public async Task ExecutarAsync_ComAprovacao_DeveAprovarOrcamentoEAvancarParaEmExecucao()
    {
        // Arrange
        var osId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var os = new OrdemServico(osId, clienteId, Guid.NewGuid(), "Troca de pastilhas");
        os.AtualizarStatus(StatusOrdemServico.EmDiagnostico);
        os.DefinirOrcamento(200.00m);
        os.AtualizarStatus(StatusOrdemServico.AguardandoAprovacao);

        var cliente = new Cliente(clienteId, "Pedro", new Cpf("12345678909"), "pedro@email.com", "11988887777");

        _ordemServicoRepositoryMock.ObterPorIdAsync(osId, Arg.Any<CancellationToken>())
            .Returns(os);
        _clienteRepositoryMock.ObterPorIdAsync(clienteId, Arg.Any<CancellationToken>())
            .Returns(cliente);

        var request = new NotificacaoOrcamentoRequest
        {
            Aprovado = true,
            Observacao = "Aprovado via webhook"
        };

        // Act
        var response = await _useCase.ExecutarAsync(osId, request, TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be("EmExecucao");
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_ComRejeicao_DeveRejeitarOrcamentoEAvancarParaCancelada()
    {
        // Arrange
        var osId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var os = new OrdemServico(osId, clienteId, Guid.NewGuid(), "Troca de pastilhas");
        os.AtualizarStatus(StatusOrdemServico.EmDiagnostico);
        os.DefinirOrcamento(200.00m);
        os.AtualizarStatus(StatusOrdemServico.AguardandoAprovacao);

        var cliente = new Cliente(clienteId, "Pedro", new Cpf("12345678909"), "pedro@email.com", "11988887777");

        _ordemServicoRepositoryMock.ObterPorIdAsync(osId, Arg.Any<CancellationToken>())
            .Returns(os);
        _clienteRepositoryMock.ObterPorIdAsync(clienteId, Arg.Any<CancellationToken>())
            .Returns(cliente);

        var request = new NotificacaoOrcamentoRequest
        {
            Aprovado = false,
            Observacao = "Valor muito alto"
        };

        // Act
        var response = await _useCase.ExecutarAsync(osId, request, TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be("Cancelada");
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
