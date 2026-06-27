using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

public interface IOrdemServicoRepository
{
    Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrdemServico>> ObterTodasAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
    Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
}
