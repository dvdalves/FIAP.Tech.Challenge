using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

[ExcludeFromCodeCoverage]
public class ServicoItemRequest
{
    public Guid? ServicoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal ValorMaoDeObra { get; set; }
}