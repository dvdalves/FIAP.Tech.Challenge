using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using FIAP.Tech.Challenge.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FIAP.Tech.Challenge.IntegrationTests.Data.Repositories;

public class VeiculoRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OficinaDbContext _context;
    private readonly VeiculoRepository _repository;

    public VeiculoRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new OficinaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new VeiculoRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AdicionarEObterPorIdAsync_DeveSalvarERecuperarVeiculo()
    {
        // Arrange
        var cliente = new Cliente(Guid.NewGuid(), "José Silva", new Cpf("12345678909"), "jose@email.com",
            "11977776666");
        var veiculo = new Veiculo(Guid.NewGuid(), new Placa("ABC1234"), "Honda", "Civic", 2020, cliente.Id);

        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();

        // Act
        await _repository.AdicionarAsync(veiculo);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var veiculoRecuperado = await _repository.ObterPorIdAsync(veiculo.Id);

        // Assert
        veiculoRecuperado.Should().NotBeNull();
        veiculoRecuperado!.Id.Should().Be(veiculo.Id);
        veiculoRecuperado.Placa.Valor.Should().Be("ABC1234");
        veiculoRecuperado.Marca.Should().Be("Honda");
        veiculoRecuperado.Modelo.Should().Be("Civic");
    }

    [Fact]
    public async Task ObterPorPlacaAsync_DeveRetornarVeiculoCorreto()
    {
        // Arrange
        var placa = new Placa("ABC1234");
        var cliente = new Cliente(Guid.NewGuid(), "José Silva", new Cpf("12345678909"), "jose@email.com",
            "11977776666");
        var veiculo = new Veiculo(Guid.NewGuid(), placa, "Honda", "Civic", 2020, cliente.Id);

        await _context.Clientes.AddAsync(cliente);
        await _repository.AdicionarAsync(veiculo);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var veiculoRecuperado = await _repository.ObterPorPlacaAsync(placa);

        // Assert
        veiculoRecuperado.Should().NotBeNull();
        veiculoRecuperado!.Id.Should().Be(veiculo.Id);
    }

    [Fact]
    public async Task ObterPorClienteIdAsync_DeveRetornarVeiculosDoCliente()
    {
        // Arrange
        var cliente = new Cliente(Guid.NewGuid(), "José Silva", new Cpf("12345678909"), "jose@email.com",
            "11977776666");
        var veiculo1 = new Veiculo(Guid.NewGuid(), new Placa("ABC1234"), "Honda", "Civic", 2020, cliente.Id);
        var veiculo2 = new Veiculo(Guid.NewGuid(), new Placa("XYZ9876"), "Toyota", "Corolla", 2022, cliente.Id);

        await _context.Clientes.AddAsync(cliente);
        await _repository.AdicionarAsync(veiculo1);
        await _repository.AdicionarAsync(veiculo2);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var veiculos = await _repository.ObterPorClienteIdAsync(cliente.Id);

        // Assert
        veiculos.Should().HaveCount(2);
    }

    [Fact]
    public async Task AtualizarAsync_DeveModificarVeiculo()
    {
        // Arrange
        var cliente = new Cliente(Guid.NewGuid(), "José Silva", new Cpf("12345678909"), "jose@email.com",
            "11977776666");
        var veiculo = new Veiculo(Guid.NewGuid(), new Placa("ABC1234"), "Honda", "Civic", 2020, cliente.Id);

        await _context.Clientes.AddAsync(cliente);
        await _repository.AdicionarAsync(veiculo);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        veiculo.AlterarModelo("City");
        await _repository.AtualizarAsync(veiculo);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();
        var recuperado = await _repository.ObterPorIdAsync(veiculo.Id);

        // Assert
        recuperado.Should().NotBeNull();
        recuperado!.Modelo.Should().Be("City");
    }
}