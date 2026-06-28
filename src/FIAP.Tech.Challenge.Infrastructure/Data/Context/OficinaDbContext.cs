using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;

using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Infrastructure.Data.Context;

[ExcludeFromCodeCoverage]
public class OficinaDbContext(DbContextOptions<OficinaDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
    public DbSet<Peca> Pecas => Set<Peca>();
    public DbSet<ItemOrdemServico> ItensOrdemServico => Set<ItemOrdemServico>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken) > 0;
    }
}
