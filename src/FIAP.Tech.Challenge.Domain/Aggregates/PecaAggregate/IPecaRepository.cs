using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;

public interface IPecaRepository
{
    Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Peca>> ObterTodasAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Peca peca, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Peca peca, CancellationToken cancellationToken = default);
}
