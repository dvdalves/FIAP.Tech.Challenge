using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;

public class Peca
{
    // EF Core constructor
    private Peca()
    {
    }

    public Peca(Guid id, string nome, decimal preco, int quantidadeEstoque)
    {
        if (id == Guid.Empty)
            throw new DominioException("Id da peça inválido.");
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome da peça é obrigatório.");
        if (preco <= 0)
            throw new DominioException("O preço da peça deve ser maior que zero.");
        if (quantidadeEstoque < 0)
            throw new DominioException("A quantidade em estoque não pode ser negativa.");

        Id = id;
        Nome = nome.Trim();
        Preco = preco;
        QuantidadeEstoque = quantidadeEstoque;
    }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int QuantidadeEstoque { get; private set; }

    public void AlterarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("O nome da peça é obrigatório.");
        Nome = nome.Trim();
    }

    public void AlterarPreco(decimal preco)
    {
        if (preco <= 0)
            throw new DominioException("O preço da peça deve ser maior que zero.");
        Preco = preco;
    }

    public void AjustarEstoque(int novaQuantidade)
    {
        if (novaQuantidade < 0)
            throw new DominioException("A quantidade em estoque não pode ser negativa.");

        QuantidadeEstoque = novaQuantidade;
    }

    public void DeduzirEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new DominioException("A quantidade a deduzir deve ser maior que zero.");
        if (QuantidadeEstoque < quantidade)
            throw new DominioException(
                $"Estoque insuficiente para a peça '{Nome}'. Disponível: {QuantidadeEstoque}, Solicitado: {quantidade}.");

        QuantidadeEstoque -= quantidade;
    }
}