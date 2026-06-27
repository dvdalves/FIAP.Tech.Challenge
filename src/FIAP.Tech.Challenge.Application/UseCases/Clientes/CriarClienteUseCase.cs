using System;
using System.Threading;
using System.Threading.Tasks;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.Clientes;

public class CriarClienteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}

public class ClienteResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}

public class CriarClienteUseCase(IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
{
    public async Task<ClienteResponse> ExecutarAsync(CriarClienteRequest request, CancellationToken cancellationToken = default)
    {
        var cpf = new Cpf(request.Cpf);

        // Verificar se cliente com mesmo CPF já existe
        var existente = await clienteRepository.ObterPorCpfAsync(cpf, cancellationToken);
        if (existente != null)
            throw new DominioException("Cliente com este CPF já está cadastrado.");

        var cliente = new Cliente(
            Guid.NewGuid(),
            request.Nome,
            cpf,
            request.Email,
            request.Telefone
        );

        await clienteRepository.AdicionarAsync(cliente, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new ClienteResponse
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Cpf = cliente.Cpf.Valor,
            Email = cliente.Email,
            Telefone = cliente.Telefone
        };
    }
}
