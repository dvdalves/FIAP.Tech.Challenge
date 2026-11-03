using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using FIAP.Tech.Challenge.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FIAP.Tech.Challenge.IntegrationTests.Data.Repositories;

public class OrdemServicoRepositoryTests
{
    private readonly OficinaDbContext _context;
    private readonly OrdemServicoRepository _repository;

    public OrdemServicoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new OficinaDbContext(options);

        _repository = new OrdemServicoRepository(_context);
    }

    [Fact]
    public async Task AdicionarEObterPorIdAsync_DeveSalvarERecuperarOrdemServico()
    {
        // Arrange
        var cliente = new Cliente(Guid.NewGuid(), "José Silva", new Cpf("12345678909"), "jose@email.com",
            "11977776666");
        var veiculo = new Veiculo(Guid.NewGuid(), new Placa("ABC1234"), "Honda", "Civic", 2020, cliente.Id);
        var os = new OrdemServico(Guid.NewGuid(), cliente.Id, veiculo.Id, "Alinhamento e balanceamento");

        // Precisamos persistir as entidades das quais a OS depende (FKs)
        await _context.Clientes.AddAsync(cliente, TestContext.Current.CancellationToken);
        await _context.Veiculos.AddAsync(veiculo, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await _repository.AdicionarAsync(os, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Limpar o rastreamento do EF para forçar uma consulta fresca ao banco
        _context.ChangeTracker.Clear();

        var osRecuperada = await _repository.ObterPorIdAsync(os.Id, TestContext.Current.CancellationToken);

        // Assert
        osRecuperada.Should().NotBeNull();
        osRecuperada!.Id.Should().Be(os.Id);
        osRecuperada.DescricaoProblema.Should().Be("Alinhamento e balanceamento");
        osRecuperada.Status.Should().Be(StatusOrdemServico.Recebida);
    }
}