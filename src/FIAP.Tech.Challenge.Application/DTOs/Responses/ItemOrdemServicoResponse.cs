using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class ItemOrdemServicoResponse
{
    public Guid Id { get; set; }
    public Guid? PecaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorMaoDeObra { get; set; }
}