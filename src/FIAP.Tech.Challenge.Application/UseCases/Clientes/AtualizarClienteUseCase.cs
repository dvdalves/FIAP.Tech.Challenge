using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.Clientes;

public class AtualizarClienteUseCase(IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
{
    public async Task<ClienteResponse> ExecutarAsync(Guid id, AtualizarClienteRequest request,
        CancellationToken cancellationToken = default)
    {
        var cliente = await clienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
            throw new DominioException("Cliente não encontrado.");

        cliente.AlterarNome(request.Nome);
        cliente.AlterarEmail(request.Email);
        cliente.AlterarTelefone(request.Telefone);

        await clienteRepository.AtualizarAsync(cliente, cancellationToken);
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
