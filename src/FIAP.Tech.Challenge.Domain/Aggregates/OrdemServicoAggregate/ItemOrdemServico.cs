using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;

public class ItemOrdemServico
{
    // EF Core constructor
    private ItemOrdemServico()
    {
    }

    public ItemOrdemServico(Guid id, Guid ordemServicoId, Guid? pecaId, string descricao, int quantidade,
        decimal valorUnitario, decimal valorMaoDeObra)
    {
        if (id == Guid.Empty)
            throw new DominioException("Id do item inválido.");
        if (ordemServicoId == Guid.Empty)
            throw new DominioException("Id da ordem de serviço é obrigatório.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DominioException("A descrição do item é obrigatória.");
        if (quantidade <= 0)
            throw new DominioException("A quantidade do item deve ser maior que zero.");
        if (valorUnitario < 0)
            throw new DominioException("O valor unitário não pode ser negativo.");
        if (valorMaoDeObra < 0)
            throw new DominioException("O valor de mão de obra não pode ser negativo.");

        Id = id;
        OrdemServicoId = ordemServicoId;
        PecaId = pecaId;
        Descricao = descricao.Trim();
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        ValorMaoDeObra = valorMaoDeObra;
    }

    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }
    public Guid? PecaId { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorMaoDeObra { get; private set; }
}