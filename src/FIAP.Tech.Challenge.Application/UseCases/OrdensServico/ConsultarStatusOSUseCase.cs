using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

/// <summary>
/// Caso de uso para consulta do status atual de uma Ordem de Serviço.
/// </summary>
public class ConsultarStatusOSUseCase(IOrdemServicoRepository ordemServicoRepository)
{
    public async Task<StatusOrdemServicoResponse> ExecutarAsync(Guid osId, CancellationToken cancellationToken = default)
    {
        var os = await ordemServicoRepository.ObterPorIdAsync(osId, cancellationToken);
        if (os == null)
            throw new DominioException("Ordem de serviço não encontrada.");

        return os.ParaStatusResponse();
    }
}
