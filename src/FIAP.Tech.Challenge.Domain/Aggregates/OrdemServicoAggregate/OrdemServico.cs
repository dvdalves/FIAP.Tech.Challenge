using System;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

public class OrdemServico
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public string DescricaoProblema { get; private set; } = string.Empty;
    public decimal ValorTotal { get; private set; }
    public StatusOrdemServico Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }

    // EF Core constructor
    private OrdemServico() { }

    public OrdemServico(Guid id, Guid clienteId, Guid veiculoId, string descricaoProblema)
    {
        if (id == Guid.Empty)
            throw new DominioException("Id da ordem de serviço inválido.");
        if (clienteId == Guid.Empty)
            throw new DominioException("Id do cliente é obrigatório.");
        if (veiculoId == Guid.Empty)
            throw new DominioException("Id do veículo é obrigatório.");
        if (string.IsNullOrWhiteSpace(descricaoProblema))
            throw new DominioException("A descrição do problema é obrigatória.");

        Id = id;
        ClienteId = clienteId;
        VeiculoId = veiculoId;
        DescricaoProblema = descricaoProblema.Trim();
        Status = StatusOrdemServico.Recebida;
        DataCriacao = DateTime.UtcNow;
        ValorTotal = 0;
    }

    public void DefinirOrcamento(decimal valor)
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
            throw new DominioException("Orçamento só pode ser definido quando a OS estiver Em Diagnóstico.");
        if (valor <= 0)
            throw new DominioException("O valor total do orçamento deve ser maior que zero.");

        ValorTotal = valor;
    }

    public void AtualizarStatus(StatusOrdemServico novoStatus)
    {
        // Validação de transições permitidas
        bool transicaoValida = (Status, novoStatus) switch
        {
            (StatusOrdemServico.Recebida, StatusOrdemServico.EmDiagnostico) => true,
            (StatusOrdemServico.EmDiagnostico, StatusOrdemServico.AguardandoAprovacao) => ValorTotal > 0, // requer orçamento definido
            (StatusOrdemServico.AguardandoAprovacao, StatusOrdemServico.EmExecucao) => true,
            (StatusOrdemServico.EmExecucao, StatusOrdemServico.Finalizada) => true,
            (StatusOrdemServico.Finalizada, StatusOrdemServico.Entregue) => true,
            _ => false
        };

        if (!transicaoValida)
        {
            if (Status == StatusOrdemServico.EmDiagnostico && novoStatus == StatusOrdemServico.AguardandoAprovacao && ValorTotal <= 0)
                throw new DominioException("Para avançar para Aguardando Aprovação, a ordem de serviço deve ter um orçamento definido.");

            throw new DominioException($"Transição de status inválida de '{Status}' para '{novoStatus}'.");
        }

        Status = novoStatus;

        if (Status == StatusOrdemServico.Finalizada || Status == StatusOrdemServico.Entregue)
        {
            DataFinalizacao = DateTime.UtcNow;
        }
    }
}
