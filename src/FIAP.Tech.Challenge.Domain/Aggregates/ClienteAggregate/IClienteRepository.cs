using FIAP.Tech.Challenge.Domain.ValueObjects;

namespace FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cliente>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
}