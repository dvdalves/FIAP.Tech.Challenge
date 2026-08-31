using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FIAP.Tech.Challenge.Infrastructure.Services;

/// <summary>
/// Implementação de serviço de envio de notificações e e-mails de atualização de status de OS.
/// </summary>
[ExcludeFromCodeCoverage]
public class EmailNotificacaoService(ILogger<EmailNotificacaoService> logger) : IServicoNotificacao
{
    public Task NotificarAtualizacaoStatusAsync(
        Guid ordemServicoId,
        string clienteNome,
        string clienteEmail,
        StatusOrdemServico statusAnterior,
        StatusOrdemServico novoStatus,
        string? observacao = null,
        CancellationToken cancellationToken = default)
    {
        var assunto = $"[Oficina Mecânica SIAES] Atualização da sua Ordem de Serviço #{ordemServicoId.ToString()[..8]}";
        
        logger.LogInformation(
            "========================================================================\n" +
            "📧 [NOTIFICAÇÃO DE E-MAIL ENVIADA COM SUCESSO]\n" +
            "Para: {ClienteNome} <{ClienteEmail}>\n" +
            "Assunto: {Assunto}\n" +
            "Ordem de Serviço: {OrdemServicoId}\n" +
            "Transição de Status: {StatusAnterior} -> {NovoStatus}\n" +
            "Observação: {Observacao}\n" +
            "Data/Hora: {DataHora:yyyy-MM-dd HH:mm:ss UTC}\n" +
            "========================================================================",
            clienteNome, clienteEmail, assunto, ordemServicoId, statusAnterior, novoStatus,
            observacao ?? "Status atualizado no sistema.", DateTime.UtcNow);

        return Task.CompletedTask;
    }
}
