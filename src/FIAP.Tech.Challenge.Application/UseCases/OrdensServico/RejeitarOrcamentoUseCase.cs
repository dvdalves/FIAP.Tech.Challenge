using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.Services;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class RejeitarOrcamentoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IClienteRepository clienteRepository,
    IServicoNotificacao servicoNotificacao,
    IUnitOfWork unitOfWork)
{
    public async Task<OrdemServicoResponse> ExecutarAsync(Guid osId, string? motivo = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Carregar a Ordem de Serviço
        var os = await ordemServicoRepository.ObterPorIdAsync(osId, cancellationToken);
        if (os == null)
            throw new DominioException("Ordem de serviço não encontrada.");

        var statusAnterior = os.Status;

        // 2. Transicionar status para Cancelada (rejeitado)
        os.AtualizarStatus(StatusOrdemServico.Cancelada);

        // 3. Persistir
        await ordemServicoRepository.AtualizarAsync(os, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        // 4. Notificar cliente
        var cliente = await clienteRepository.ObterPorIdAsync(os.ClienteId, cancellationToken);
        if (cliente != null)
        {
            await servicoNotificacao.NotificarAtualizacaoStatusAsync(
                os.Id,
                cliente.Nome,
                cliente.Email,
                statusAnterior,
                StatusOrdemServico.Cancelada,
                motivo ?? "Orçamento rejeitado pelo cliente. Ordem de serviço cancelada.",
                cancellationToken);
        }

        return os.ParaResponse();
    }
}