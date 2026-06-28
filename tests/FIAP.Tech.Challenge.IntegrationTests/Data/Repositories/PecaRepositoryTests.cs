using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using FIAP.Tech.Challenge.Infrastructure.Repositories;

namespace FIAP.Tech.Challenge.IntegrationTests.Data.Repositories;

public class PecaRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OficinaDbContext _context;
    private readonly PecaRepository _repository;

    public PecaRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<OficinaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new OficinaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new PecaRepository(_context);
    }

    [Fact]
    public async Task AdicionarEObterPorIdAsync_DeveSalvarERecuperarPeca()
    {
        // Arrange
        var peca = new Peca(Guid.NewGuid(), "Filtro de Óleo", 59.90m, 15);

        // Act
        await _repository.AdicionarAsync(peca);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var pecaRecuperada = await _repository.ObterPorIdAsync(peca.Id);

        // Assert
        pecaRecuperada.Should().NotBeNull();
        pecaRecuperada!.Id.Should().Be(peca.Id);
        pecaRecuperada.Nome.Should().Be("Filtro de Óleo");
        pecaRecuperada.Preco.Should().Be(59.90m);
        pecaRecuperada.QuantidadeEstoque.Should().Be(15);
    }

    [Fact]
    public async Task ObterTodasAsync_DeveRetornarTodasAsPecas()
    {
        // Arrange
        var peca1 = new Peca(Guid.NewGuid(), "Peca A", 10.00m, 5);
        var peca2 = new Peca(Guid.NewGuid(), "Peca B", 20.00m, 10);

        await _repository.AdicionarAsync(peca1);
        await _repository.AdicionarAsync(peca2);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        var todas = await _repository.ObterTodasAsync();

        // Assert
        todas.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task AtualizarAsync_DeveModificarPeca()
    {
        // Arrange
        var peca = new Peca(Guid.NewGuid(), "Filtro de Óleo", 59.90m, 15);
        await _repository.AdicionarAsync(peca);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        // Act
        peca.AjustarEstoque(30);
        await _repository.AtualizarAsync(peca);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();
        var recuperada = await _repository.ObterPorIdAsync(peca.Id);

        // Assert
        recuperada.Should().NotBeNull();
        recuperada!.QuantidadeEstoque.Should().Be(30);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
