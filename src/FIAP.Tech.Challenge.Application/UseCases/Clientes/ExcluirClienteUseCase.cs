using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.Clientes;

public class ExcluirClienteUseCase(
    IClienteRepository clienteRepository,
    IOrdemServicoRepository ordemServicoRepository,
    IUnitOfWork unitOfWork)
{
    public async Task ExecutarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await clienteRepository.ObterPorIdAsync(id, cancellationToken);
        if (cliente == null)
            throw new DominioException("Cliente não encontrado.");

        if (cliente.Veiculos.Any())
            throw new DominioException("Não é possível excluir um cliente com veículos vinculados.");

        var ordens = await ordemServicoRepository.ObterTodasAsync(cancellationToken);
        if (ordens.Any(os => os.ClienteId == id))
            throw new DominioException("Não é possível excluir um cliente com ordens de serviço vinculadas.");

        clienteRepository.Remover(cliente);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
