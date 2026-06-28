using FIAP.Tech.Challenge.Application.UseCases.Clientes;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FIAP.Tech.Challenge.UnitTests.Application.UseCases.Clientes;

public class ExcluirClienteUseCaseTests
{
    private readonly IClienteRepository _clienteRepositoryMock;
    private readonly IOrdemServicoRepository _ordemServicoRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ExcluirClienteUseCase _useCase;

    public ExcluirClienteUseCaseTests()
    {
        _clienteRepositoryMock = Substitute.For<IClienteRepository>();
        _ordemServicoRepositoryMock = Substitute.For<IOrdemServicoRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _useCase = new ExcluirClienteUseCase(_clienteRepositoryMock, _ordemServicoRepositoryMock, _unitOfWorkMock);
    }

    [Fact]
    public async Task ExecutarAsync_ClienteInexistente_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _clienteRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Cliente)null!);

        // Act
        var act = () => _useCase.ExecutarAsync(id);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Cliente não encontrado.");

        _clienteRepositoryMock.DidNotReceive().Remover(Arg.Any<Cliente>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_ClientePossuiVeiculos_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cliente = new Cliente(id, "Cliente Teste", new Cpf("12345678909"), "teste@email.com", "11999999999");
        var veiculo = new Veiculo(Guid.NewGuid(), new Placa("ABC1D23"), "Ford", "Ka", 2020, id);
        
        // Simular vinculação de veículo ao cliente
        var veiculosProperty = cliente.GetType().GetField("_veiculos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var veiculosList = (List<Veiculo>)veiculosProperty!.GetValue(cliente)!;
        veiculosList.Add(veiculo);

        _clienteRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(cliente);

        // Act
        var act = () => _useCase.ExecutarAsync(id);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Não é possível excluir um cliente com veículos vinculados.");

        _clienteRepositoryMock.DidNotReceive().Remover(Arg.Any<Cliente>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_ClientePossuiOrdemServico_DeveLancarDominioException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cliente = new Cliente(id, "Cliente Teste", new Cpf("12345678909"), "teste@email.com", "11999999999");
        
        _clienteRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(cliente);

        var ordens = new List<OrdemServico>
        {
            new OrdemServico(Guid.NewGuid(), id, Guid.NewGuid(), "Troca de óleo")
        };

        _ordemServicoRepositoryMock.ObterTodasAsync(Arg.Any<CancellationToken>())
            .Returns(ordens);

        // Act
        var act = () => _useCase.ExecutarAsync(id);

        // Assert
        await act.Should().ThrowAsync<DominioException>()
            .WithMessage("Não é possível excluir um cliente com ordens de serviço vinculadas.");

        _clienteRepositoryMock.DidNotReceive().Remover(Arg.Any<Cliente>());
        await _unitOfWorkMock.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecutarAsync_SemVinculos_DeveRemoverECommitar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cliente = new Cliente(id, "Cliente Teste", new Cpf("12345678909"), "teste@email.com", "11999999999");

        _clienteRepositoryMock.ObterPorIdAsync(id, Arg.Any<CancellationToken>())
            .Returns(cliente);

        _ordemServicoRepositoryMock.ObterTodasAsync(Arg.Any<CancellationToken>())
            .Returns(new List<OrdemServico>());

        // Act
        await _useCase.ExecutarAsync(id, TestContext.Current.CancellationToken);

        // Assert
        _clienteRepositoryMock.Received(1).Remover(cliente);
        await _unitOfWorkMock.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
