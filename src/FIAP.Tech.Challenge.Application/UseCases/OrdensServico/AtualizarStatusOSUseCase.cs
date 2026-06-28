using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class AtualizarStatusOSUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<OrdemServicoResponse> ExecutarAsync(Guid id, StatusOrdemServico novoStatus,
        CancellationToken cancellationToken = default)
    {
        var os = await ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (os == null)
            throw new DominioException("Ordem de serviço não encontrada.");

        // Executa a transição de status validando as regras do domínio
        os.AtualizarStatus(novoStatus);

        await ordemServicoRepository.AtualizarAsync(os, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return os.ParaResponse();
    }
}