using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

[ExcludeFromCodeCoverage]
public class AbrirOrdemServicoRequest
{
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string DescricaoProblema { get; set; } = string.Empty;
}