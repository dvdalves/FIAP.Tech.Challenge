using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

namespace FIAP.Tech.Challenge.Domain.Services;

/// <summary>
/// Contrato para serviço de envio de notificações e e-mails aos clientes da oficina.
/// </summary>
public interface IServicoNotificacao
{
    /// <summary>
    /// Notifica o cliente proprietário sobre a alteração de status de sua Ordem de Serviço.
    /// </summary>
    Task NotificarAtualizacaoStatusAsync(
        Guid ordemServicoId,
        string clienteNome,
        string clienteEmail,
        StatusOrdemServico statusAnterior,
        StatusOrdemServico novoStatus,
        string? observacao = null,
        CancellationToken cancellationToken = default);
}
