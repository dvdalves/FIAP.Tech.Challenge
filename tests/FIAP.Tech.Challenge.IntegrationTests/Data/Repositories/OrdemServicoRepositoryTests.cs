using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using FIAP.Tech.Challenge.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FIAP.Tech.Challenge.IntegrationTests.Data.Repositories;

public class OrdemServicoRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OficinaDbContext _context;
    private readonly OrdemServicoRepository _repository;

    public OrdemServicoRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new OficinaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new OrdemServicoRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
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
        await _context.Clientes.AddAsync(cliente);
        await _context.Veiculos.AddAsync(veiculo);
        await _context.SaveChangesAsync();

        // Act
        await _repository.AdicionarAsync(os);
        await _context.SaveChangesAsync();

        // Limpar o rastreamento do EF para forçar uma consulta fresca ao banco
        _context.ChangeTracker.Clear();

        var osRecuperada = await _repository.ObterPorIdAsync(os.Id);

        // Assert
        osRecuperada.Should().NotBeNull();
        osRecuperada!.Id.Should().Be(os.Id);
        osRecuperada.DescricaoProblema.Should().Be("Alinhamento e balanceamento");
        osRecuperada.Status.Should().Be(StatusOrdemServico.Recebida);
    }
}