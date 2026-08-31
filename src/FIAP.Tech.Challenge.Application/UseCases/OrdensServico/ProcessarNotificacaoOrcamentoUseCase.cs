using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.DTOs.Responses;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

/// <summary>
/// Caso de uso para processamento de notificações externas (webhook) de aprovação ou recusa do orçamento.
/// </summary>
public class ProcessarNotificacaoOrcamentoUseCase(
    AprovarOrcamentoUseCase aprovarOrcamentoUseCase,
    RejeitarOrcamentoUseCase rejeitarOrcamentoUseCase)
{
    public async Task<OrdemServicoResponse> ExecutarAsync(
        Guid osId,
        NotificacaoOrcamentoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Aprovado)
        {
            return await aprovarOrcamentoUseCase.ExecutarAsync(osId, cancellationToken);
        }
        else
        {
            return await rejeitarOrcamentoUseCase.ExecutarAsync(osId, request.Observacao, cancellationToken);
        }
    }
}
