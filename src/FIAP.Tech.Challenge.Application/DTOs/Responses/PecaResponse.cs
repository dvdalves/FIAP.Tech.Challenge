using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class PecaResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}