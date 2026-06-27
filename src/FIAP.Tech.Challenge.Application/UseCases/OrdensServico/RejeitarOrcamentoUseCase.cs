using System;
using System.Threading;
using System.Threading.Tasks;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class RejeitarOrcamentoUseCase(IOrdemServicoRepository ordemServicoRepository, IUnitOfWork unitOfWork)
{
    public async Task<OrdemServicoResponse> ExecutarAsync(Guid osId, CancellationToken cancellationToken = default)
    {
        // 1. Carregar a Ordem de Serviço
        var os = await ordemServicoRepository.ObterPorIdAsync(osId, cancellationToken);
        if (os == null)
            throw new DominioException("Ordem de serviço não encontrada.");

        // 2. Transicionar status para Cancelada (rejeitado)
        os.AtualizarStatus(StatusOrdemServico.Cancelada);

        // 3. Persistir
        await ordemServicoRepository.AtualizarAsync(os, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return os.ParaResponse();
    }
}
