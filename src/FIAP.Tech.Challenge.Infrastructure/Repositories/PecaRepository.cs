using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.Tech.Challenge.Infrastructure.Repositories;

public class PecaRepository(OficinaDbContext context) : IPecaRepository
{
    public async Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Pecas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Peca>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        return await context.Pecas.ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Peca peca, CancellationToken cancellationToken = default)
    {
        await context.Pecas.AddAsync(peca, cancellationToken);
    }

    public Task AtualizarAsync(Peca peca, CancellationToken cancellationToken = default)
    {
        var entry = context.Entry(peca);
        if (entry.State == EntityState.Detached) context.Pecas.Update(peca);
        return Task.CompletedTask;
    }

    public void Remover(Peca peca)
    {
        context.Pecas.Remove(peca);
    }
}