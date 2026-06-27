using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;

namespace FIAP.Tech.Challenge.Infrastructure.Repositories;

public class OrdemServicoRepository(OficinaDbContext context) : IOrdemServicoRepository
{
    public async Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.OrdensServico
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<OrdemServico>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        return await context.OrdensServico
            .Include(o => o.Itens)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        await context.OrdensServico.AddAsync(ordemServico, cancellationToken);
    }

    public async Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        var entry = context.Entry(ordemServico);
        if (entry.State == EntityState.Detached)
        {
            context.OrdensServico.Update(ordemServico);
        }

        foreach (var item in ordemServico.Itens)
        {
            var itemEntry = context.Entry(item);
            if (itemEntry.State == EntityState.Modified || itemEntry.State == EntityState.Detached)
            {
                var exists = await context.ItensOrdemServico.AnyAsync(i => i.Id == item.Id, cancellationToken);
                if (!exists)
                {
                    itemEntry.State = EntityState.Added;
                }
            }
        }
    }
}
