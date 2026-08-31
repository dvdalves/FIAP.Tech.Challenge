using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

namespace FIAP.Tech.Challenge.Application.Mappings;

[ExcludeFromCodeCoverage]
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
            DataFinalizacao = os.DataFinalizacao,
            Itens = os.Itens?.Select(i => new ItemOrdemServicoResponse
            {
                Id = i.Id,
                PecaId = i.PecaId,
                Descricao = i.Descricao,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario,
                ValorMaoDeObra = i.ValorMaoDeObra
            }).ToList() ?? []
        };
    }

    public static StatusOrdemServicoResponse ParaStatusResponse(this OrdemServico os)
    {
        if (os == null) return null!;

        return new StatusOrdemServicoResponse
        {
            OrdemServicoId = os.Id,
            Status = os.Status.ToString(),
            DescricaoStatus = ObterDescricaoStatus(os.Status),
            ValorTotal = os.ValorTotal,
            DataCriacao = os.DataCriacao,
            DataInicioExecucao = os.DataInicioExecucao,
            DataFinalizacao = os.DataFinalizacao
        };
    }

    public static string ObterDescricaoStatus(StatusOrdemServico status) => status switch
    {
        StatusOrdemServico.Recebida => "Recebida na Recepção",
        StatusOrdemServico.EmDiagnostico => "Em Diagnóstico Técnico",
        StatusOrdemServico.AguardandoAprovacao => "Aguardando Aprovação do Cliente",
        StatusOrdemServico.EmExecucao => "Em Execução / Manutenção",
        StatusOrdemServico.Finalizada => "Finalizada (Pronta para Retirada)",
        StatusOrdemServico.Entregue => "Entregue ao Cliente",
        StatusOrdemServico.Cancelada => "Cancelada",
        _ => status.ToString()
    };
}