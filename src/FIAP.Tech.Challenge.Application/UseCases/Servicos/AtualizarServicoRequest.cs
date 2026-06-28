using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.UseCases.Servicos;

[ExcludeFromCodeCoverage]
public class AtualizarServicoRequest
{
    public string Nome { get; set; } = string.Empty;
    public decimal PrecoMaoDeObra { get; set; }
}
