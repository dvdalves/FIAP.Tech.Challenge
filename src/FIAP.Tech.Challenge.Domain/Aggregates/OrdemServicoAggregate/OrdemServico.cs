using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

public class OrdemServico
{
    private readonly List<ItemOrdemServico> _itens = new();

    // EF Core constructor
    private OrdemServico()
    {
    }

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

    public Guid Id { get; }
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public string DescricaoProblema { get; private set; } = string.Empty;
    public decimal ValorTotal { get; private set; }
    public StatusOrdemServico Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataInicioExecucao { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public IReadOnlyCollection<ItemOrdemServico> Itens => _itens.AsReadOnly();

    public void DefinirOrcamento(decimal valor)
    {
        if (Status != StatusOrdemServico.EmDiagnostico)
            throw new DominioException("Orçamento só pode ser definido quando a OS estiver Em Diagnóstico.");
        if (valor <= 0)
            throw new DominioException("O valor total do orçamento deve ser maior que zero.");

        ValorTotal = valor;
    }

    public void AdicionarItem(Guid? pecaId, string descricao, int quantidade, decimal valorUnitario,
        decimal valorMaoDeObra)
    {
        if (Status != StatusOrdemServico.Recebida && Status != StatusOrdemServico.EmDiagnostico)
            throw new DominioException(
                "Itens só podem ser adicionados nos status iniciais (Recebida ou Em Diagnóstico).");

        if (quantidade <= 0)
            throw new DominioException("A quantidade do item deve ser maior que zero.");
        if (valorUnitario < 0)
            throw new DominioException("O valor unitário não pode ser negativo.");
        if (valorMaoDeObra < 0)
            throw new DominioException("O valor de mão de obra não pode ser negativo.");

        var item = new ItemOrdemServico(Guid.NewGuid(), Id, pecaId, descricao, quantidade, valorUnitario,
            valorMaoDeObra);
        _itens.Add(item);

        // Recalcular o valor total automaticamente
        RecalcularValorTotal();
    }

    public void FinalizarDiagnostico()
    {
        if (Status != StatusOrdemServico.Recebida && Status != StatusOrdemServico.EmDiagnostico)
            throw new DominioException(
                "O diagnóstico só pode ser finalizado nos status iniciais (Recebida ou Em Diagnóstico).");
        if (_itens.Count == 0 && ValorTotal <= 0)
            throw new DominioException(
                "A ordem de serviço deve conter pelo menos um item ou orçamento para finalizar o diagnóstico.");

        // Se o valor total for zero mas tiver itens, recalcula (garantia)
        if (_itens.Count > 0) RecalcularValorTotal();

        if (ValorTotal <= 0)
            throw new DominioException("O valor total do orçamento deve ser maior que zero.");

        // Transiciona status para Aguardando Aprovação
        AtualizarStatus(StatusOrdemServico.AguardandoAprovacao);
    }

    private void RecalcularValorTotal()
    {
        decimal total = 0;
        foreach (var item in _itens) total += item.ValorUnitario * item.Quantidade + item.ValorMaoDeObra;
        ValorTotal = total;
    }

    public void AtualizarStatus(StatusOrdemServico novoStatus)
    {
        // Validação de transições permitidas
        var transicaoValida = (Status, novoStatus) switch
        {
            (StatusOrdemServico.Recebida, StatusOrdemServico.EmDiagnostico) => true,
            (StatusOrdemServico.Recebida, StatusOrdemServico.Cancelada) => true,
            (StatusOrdemServico.EmDiagnostico, StatusOrdemServico
                .AguardandoAprovacao) => ValorTotal > 0, // requer orçamento definido
            (StatusOrdemServico.EmDiagnostico, StatusOrdemServico.Cancelada) => true,
            (StatusOrdemServico.AguardandoAprovacao, StatusOrdemServico.EmExecucao) => true,
            (StatusOrdemServico.AguardandoAprovacao, StatusOrdemServico.Cancelada) => true,
            (StatusOrdemServico.EmExecucao, StatusOrdemServico.Finalizada) => true,
            (StatusOrdemServico.Finalizada, StatusOrdemServico.Entregue) => true,
            _ => false
        };

        if (!transicaoValida)
        {
            if (Status == StatusOrdemServico.EmDiagnostico && novoStatus == StatusOrdemServico.AguardandoAprovacao &&
                ValorTotal <= 0)
                throw new DominioException(
                    "Para avançar para Aguardando Aprovação, a ordem de serviço deve ter um orçamento definido.");

            throw new DominioException($"Transição de status inválida de '{Status}' para '{novoStatus}'.");
        }

        Status = novoStatus;

        if (Status == StatusOrdemServico.EmExecucao) DataInicioExecucao = DateTime.UtcNow;

        if (Status == StatusOrdemServico.Finalizada || Status == StatusOrdemServico.Entregue ||
            Status == StatusOrdemServico.Cancelada) DataFinalizacao = DateTime.UtcNow;
    }
}