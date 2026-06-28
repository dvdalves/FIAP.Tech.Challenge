using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.Tech.Challenge.Infrastructure.Repositories;

public class ServicoRepository(OficinaDbContext context) : IServicoRepository
{
    public async Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Servicos.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Servico>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await context.Servicos.ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        await context.Servicos.AddAsync(servico, cancellationToken);
    }

    public Task AtualizarAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        context.Servicos.Update(servico);
        return Task.CompletedTask;
    }

    public void Remover(Servico servico)
    {
        context.Servicos.Remove(servico);
    }
}
