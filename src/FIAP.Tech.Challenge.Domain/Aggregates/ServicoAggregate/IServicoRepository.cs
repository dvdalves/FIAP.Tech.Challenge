namespace FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;

public interface IServicoRepository
{
    Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Servico>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Servico servico, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Servico servico, CancellationToken cancellationToken = default);
    void Remover(Servico servico);
}
