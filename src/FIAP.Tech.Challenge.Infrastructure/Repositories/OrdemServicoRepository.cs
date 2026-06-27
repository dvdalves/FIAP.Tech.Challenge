using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;

namespace FIAP.Tech.Challenge.Infrastructure.Repositories;

public class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly OficinaDbContext _context;

    public OrdemServicoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.OrdensServico.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<OrdemServico>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OrdensServico.ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        await _context.OrdensServico.AddAsync(ordemServico, cancellationToken);
    }

    public Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        _context.OrdensServico.Update(ordemServico);
        return Task.CompletedTask;
    }
}
