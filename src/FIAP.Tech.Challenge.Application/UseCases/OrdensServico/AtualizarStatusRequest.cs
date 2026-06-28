using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

[ExcludeFromCodeCoverage]
public class AtualizarStatusRequest
{
    public StatusOrdemServico NovoStatus { get; set; }
}