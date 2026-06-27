using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;

namespace FIAP.Tech.Challenge.Infrastructure.Repositories;

public class VeiculoRepository(OficinaDbContext context) : IVeiculoRepository
{
    public async Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Veiculos.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Veiculo?> ObterPorPlacaAsync(Placa placa, CancellationToken cancellationToken = default)
    {
        return await context.Veiculos.FirstOrDefaultAsync(v => v.Placa == placa, cancellationToken);
    }

    public async Task<IEnumerable<Veiculo>> ObterPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await context.Veiculos.Where(v => v.ClienteId == clienteId).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        await context.Veiculos.AddAsync(veiculo, cancellationToken);
    }

    public Task AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        context.Veiculos.Update(veiculo);
        return Task.CompletedTask;
    }
}
