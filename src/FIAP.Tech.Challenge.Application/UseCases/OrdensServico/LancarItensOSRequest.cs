using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

[ExcludeFromCodeCoverage]
public class LancarItensOSRequest
{
    public List<PecaItemRequest> Pecas { get; set; } = new();
    public List<ServicoItemRequest> Servicos { get; set; } = new();
}