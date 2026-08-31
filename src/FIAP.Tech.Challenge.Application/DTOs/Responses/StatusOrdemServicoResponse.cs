using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class StatusOrdemServicoResponse
{
    public Guid OrdemServicoId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DescricaoStatus { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataInicioExecucao { get; set; }
    public DateTime? DataFinalizacao { get; set; }
}
