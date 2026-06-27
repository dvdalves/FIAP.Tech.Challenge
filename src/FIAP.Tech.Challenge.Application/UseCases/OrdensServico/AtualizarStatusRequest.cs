using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class AtualizarStatusRequest
{
    public StatusOrdemServico NovoStatus { get; set; }
}