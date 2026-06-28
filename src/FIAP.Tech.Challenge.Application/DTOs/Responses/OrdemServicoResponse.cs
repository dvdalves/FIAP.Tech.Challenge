using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Application.DTOs.Responses;

[ExcludeFromCodeCoverage]
public class OrdemServicoResponse
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string DescricaoProblema { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime? DataFinalizacao { get; set; }
    public List<ItemOrdemServicoResponse> Itens { get; set; } = new();
}