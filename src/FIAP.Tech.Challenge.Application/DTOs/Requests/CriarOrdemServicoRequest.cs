using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

namespace FIAP.Tech.Challenge.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public class CriarOrdemServicoRequest
{
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteCpf { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClienteTelefone { get; set; } = string.Empty;

    public string VeiculoPlaca { get; set; } = string.Empty;
    public string VeiculoMarca { get; set; } = string.Empty;
    public string VeiculoModelo { get; set; } = string.Empty;
    public int VeiculoAno { get; set; }

    public string DescricaoProblema { get; set; } = string.Empty;

    public List<PecaItemRequest>? ItensPeca { get; set; } = [];
    public List<ServicoItemRequest>? ItensServico { get; set; } = [];
}