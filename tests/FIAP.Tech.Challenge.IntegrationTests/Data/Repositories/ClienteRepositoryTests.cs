using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using FIAP.Tech.Challenge.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FIAP.Tech.Challenge.IntegrationTests.Data.Repositories;

public class ClienteRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OficinaDbContext _context;
    private readonly ClienteRepository _repository;

    public ClienteRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new OficinaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new ClienteRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AdicionarEObterPorIdAsync_DeveSalvarERecuperarCliente()
    {
        // Arrange
        var cliente = new Cliente(Guid.NewGuid(), "José Silva", new Cpf("12345678909"), "jose@email.com",
            "11977776666");

        // Act
        await _repository.AdicionarAsync(cliente, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _context.ChangeTracker.Clear();

        var clienteRecuperado = await _repository.ObterPorIdAsync(cliente.Id, TestContext.Current.CancellationToken);

        // Assert
        clienteRecuperado.Should().NotBeNull();
        clienteRecuperado!.Id.Should().Be(cliente.Id);
        clienteRecuperado.Nome.Should().Be("José Silva");
        clienteRecuperado.Cpf.Valor.Should().Be("12345678909");
    }

    [Fact]
    public async Task ObterPorCpfAsync_DeveRetornarClienteCorreto()
    {
        // Arrange
        var cpf = new Cpf("12345678909");
        var cliente = new Cliente(Guid.NewGuid(), "José Silva", cpf, "jose@email.com", "11977776666");

        await _repository.AdicionarAsync(cliente, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _context.ChangeTracker.Clear();

        // Act
        var clienteRecuperado = await _repository.ObterPorCpfAsync(cpf, TestContext.Current.CancellationToken);

        // Assert
        clienteRecuperado.Should().NotBeNull();
        clienteRecuperado!.Id.Should().Be(cliente.Id);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarTodosOsClientes()
    {
        // Arrange
        var cliente1 = new Cliente(Guid.NewGuid(), "José Silva", new Cpf("12345678909"), "jose@email.com",
            "11977776666");
        var cliente2 = new Cliente(Guid.NewGuid(), "Maria Santos", new Cpf("98765432100"), "maria@email.com",
            "11988887777");

        await _repository.AdicionarAsync(cliente1, TestContext.Current.CancellationToken);
        await _repository.AdicionarAsync(cliente2, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _context.ChangeTracker.Clear();

        // Act
        var todos = await _repository.ObterTodosAsync(TestContext.Current.CancellationToken);

        // Assert
        todos.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task AtualizarAsync_DeveModificarCliente()
    {
        // Arrange
        var cliente = new Cliente(Guid.NewGuid(), "José Silva", new Cpf("12345678909"), "jose@email.com",
            "11977776666");
        await _repository.AdicionarAsync(cliente, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _context.ChangeTracker.Clear();

        // Act
        cliente.AlterarEmail("novo@email.com");
        await _repository.AtualizarAsync(cliente, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _context.ChangeTracker.Clear();
        var recuperado = await _repository.ObterPorIdAsync(cliente.Id, TestContext.Current.CancellationToken);

        // Assert
        recuperado.Should().NotBeNull();
        recuperado!.Email.Should().Be("novo@email.com");
    }
}