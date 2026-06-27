using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using NSubstitute;
using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.OrdensServico;

public class CriarOrdemServicoUseCaseTests
{
    private readonly IClienteRepository _clienteRepositoryMock;
    private readonly IVeiculoRepository _veiculoRepositoryMock;
    private readonly IOrdemServicoRepository _ordemServicoRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly CriarOrdemServicoUseCase _useCase;

    public CriarOrdemServicoUseCaseTests()
    {
        _clienteRepositoryMock = Substitute.For<IClienteRepository>();
        _veiculoRepositoryMock = Substitute.For<IVeiculoRepository>();
        _ordemServicoRepositoryMock = Substitute.For<IOrdemServicoRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _useCase = new CriarOrdemServicoUseCase(
            _clienteRepositoryMock,
            _veiculoRepositoryMock,
            _ordemServicoRepositoryMock,
            _unitOfWorkMock
        );
    }

    [Fact]
    public async Task ExecutarAsync_ComClienteEVeiculoNovos_DeveCriarEGravarTudo()
    {
        // Arrange
        var request = new CriarOrdemServicoRequest
        {
            ClienteNome = "Guilherme Santos",
            ClienteCpf = "12345678909",
            ClienteEmail = "gui@email.com",
            ClienteTelefone = "11988887777",
            VeiculoPlaca = "ABC-1234",
            VeiculoMarca = "Fiat",
            VeiculoModelo = "Uno",
            VeiculoAno = 2015,
            DescricaoProblema = "Barulho na suspensão"
        };

        _clienteRepositoryMock.ObterPorCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>())
            .Returns((Cliente)null!);
        _veiculoRepositoryMock.ObterPorPlacaAsync(Arg.Any<Placa>(), Arg.Any<CancellationToken>())
            .Returns((Veiculo)null!);
        _unitOfWorkMock.CommitAsync(Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await _useCase.ExecutarAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.DescricaoProblema.Should().Be("Barulho na suspensão");
        response.Status.Should().Be("Recebida");

        // Verificar chamadas aos mocks
        await _clienteRepositoryMock.Received(1).AdicionarAsync(Arg.Any<Cliente>(), Arg.Any<CancellationToken>());
        await _veiculoRepositoryMock.Received(1).AdicionarAsync(Arg.Any<Veiculo>(), Arg.Any<CancellationToken>());
        await _ordemServicoRepositoryMock.Received(1).AdicionarAsync(Arg.Any<OrdemServico>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_ComClienteEVeiculoExistentes_DeveApenasCriarOrdemServico()
    {
        // Arrange
        var clienteExistente = new Cliente(Guid.NewGuid(), "Carlos", new Cpf("12345678909"), "carlos@email.com", "11988887777");
        var veiculoExistente = new Veiculo(Guid.NewGuid(), new Placa("ABC1234"), "Ford", "Ka", 2018, clienteExistente.Id);

        var request = new CriarOrdemServicoRequest
        {
            ClienteCpf = "12345678909",
            VeiculoPlaca = "ABC1234",
            DescricaoProblema = "Revisão geral"
        };

        _clienteRepositoryMock.ObterPorCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>())
            .Returns(clienteExistente);
        _veiculoRepositoryMock.ObterPorPlacaAsync(Arg.Any<Placa>(), Arg.Any<CancellationToken>())
            .Returns(veiculoExistente);

        // Act
        var response = await _useCase.ExecutarAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.ClienteId.Should().Be(clienteExistente.Id);
        response.VeiculoId.Should().Be(veiculoExistente.Id);

        // Não deve criar novos registros para cliente/veiculo
        await _clienteRepositoryMock.DidNotReceive().AdicionarAsync(Arg.Any<Cliente>(), Arg.Any<CancellationToken>());
        await _veiculoRepositoryMock.DidNotReceive().AdicionarAsync(Arg.Any<Veiculo>(), Arg.Any<CancellationToken>());

        // Mas deve criar OS
        await _ordemServicoRepositoryMock.Received(1).AdicionarAsync(Arg.Any<OrdemServico>(), Arg.Any<CancellationToken>());
    }
}
