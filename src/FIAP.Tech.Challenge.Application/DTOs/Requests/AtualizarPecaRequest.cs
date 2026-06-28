using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public class AtualizarPecaRequest
{
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}
