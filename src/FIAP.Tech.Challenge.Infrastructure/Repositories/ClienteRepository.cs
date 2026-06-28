using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.Tech.Challenge.Infrastructure.Repositories;

public class ClienteRepository(OficinaDbContext context) : IClienteRepository
{
    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Clientes
            .Include(c => c.Veiculos)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cliente?> ObterPorCpfAsync(Cpf cpf, CancellationToken cancellationToken = default)
    {
        return await context.Clientes
            .Include(c => c.Veiculos)
            .FirstOrDefaultAsync(c => c.Cpf == cpf, cancellationToken);
    }

    public async Task<IEnumerable<Cliente>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await context.Clientes
            .Include(c => c.Veiculos)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await context.Clientes.AddAsync(cliente, cancellationToken);
    }

    public Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        context.Clientes.Update(cliente);
        return Task.CompletedTask;
    }
}