using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;

public class Servico
{
    private Servico() { }

    public Servico(Guid id, string nome, decimal precoMaoDeObra)
    {
        if (id == Guid.Empty)
            throw new DominioException("Id do serviço inválido.");
        AlterarNome(nome);
        AlterarPreco(precoMaoDeObra);

        Id = id;
    }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public decimal PrecoMaoDeObra { get; private set; }

    public void AlterarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("Nome do serviço é obrigatório.");
        Nome = nome.Trim();
    }

    public void AlterarPreco(decimal precoMaoDeObra)
    {
        if (precoMaoDeObra < 0)
            throw new DominioException("O preço da mão de obra não pode ser negativo.");
        PrecoMaoDeObra = precoMaoDeObra;
    }
}
