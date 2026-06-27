using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

namespace FIAP.Tech.Challenge.Application.Mappings;

public static class PerfilMapping
{
    public static OrdemServicoResponse ParaResponse(this OrdemServico os)
    {
        if (os == null) return null!;

        return new OrdemServicoResponse
        {
            Id = os.Id,
            ClienteId = os.ClienteId,
            VeiculoId = os.VeiculoId,
            DescricaoProblema = os.DescricaoProblema,
            ValorTotal = os.ValorTotal,
            Status = os.Status.ToString(),
            DataCriacao = os.DataCriacao,
            DataFinalizacao = os.DataFinalizacao
        };
    }
}
