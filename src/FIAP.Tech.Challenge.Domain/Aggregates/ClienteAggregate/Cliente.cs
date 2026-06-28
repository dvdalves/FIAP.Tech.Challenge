using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.ValueObjects;

namespace FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;

public class Cliente
{
    // EF Core constructor
    private Cliente()
    {
    }

    public Cliente(Guid id, string nome, Cpf cpf, string email, string telefone)
    {
        if (id == Guid.Empty)
            throw new DominioException("Id do cliente inválido.");

        AlterarNome(nome);
        AlterarCpf(cpf);
        AlterarEmail(email);
        AlterarTelefone(telefone);

        Id = id;
    }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public Cpf Cpf { get; private set; } = null!;
    public string Email { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;

    public void AlterarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DominioException("Nome do cliente é obrigatório.");
        Nome = nome.Trim();
    }

    public void AlterarCpf(Cpf cpf)
    {
        Cpf = cpf ?? throw new DominioException("CPF do cliente é obrigatório.");
    }

    public void AlterarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DominioException("E-mail do cliente é obrigatório.");
        if (!email.Contains('@'))
            throw new DominioException("E-mail do cliente é inválido.");
        Email = email.Trim().ToLower();
    }

    public void AlterarTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new DominioException("Telefone do cliente é obrigatório.");
        Telefone = telefone.Trim();
    }
}