using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

[ExcludeFromCodeCoverage]
public class AtualizarStatusRequest
{
    public StatusOrdemServico NovoStatus { get; set; }
}