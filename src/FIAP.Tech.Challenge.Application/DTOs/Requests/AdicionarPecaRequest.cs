using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public class AdicionarPecaRequest
{
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
}
