using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

[ExcludeFromCodeCoverage]
public class PecaItemRequest
{
    public Guid PecaId { get; set; }
    public int Quantidade { get; set; }
}