using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FIAP.Tech.Challenge.Domain.ValueObjects;

namespace FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Veiculo?> ObterPorPlacaAsync(Placa placa, CancellationToken cancellationToken = default);
    Task<IEnumerable<Veiculo>> ObterPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
}
