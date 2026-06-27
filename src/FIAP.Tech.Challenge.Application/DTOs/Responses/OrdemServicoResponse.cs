using System;
using System.Collections.Generic;

namespace FIAP.Tech.Challenge.Application.DTOs.Responses;

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

public class ItemOrdemServicoResponse
{
    public Guid Id { get; set; }
    public Guid? PecaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorMaoDeObra { get; set; }
}
