using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.Clientes;

public class ExcluirVeiculoUseCase(
    IVeiculoRepository veiculoRepository,
    IOrdemServicoRepository ordemServicoRepository,
    IUnitOfWork unitOfWork)
{
    public async Task ExecutarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var veiculo = await veiculoRepository.ObterPorIdAsync(id, cancellationToken);
        if (veiculo == null)
            throw new DominioException("Veículo não encontrado.");

        var ordens = await ordemServicoRepository.ObterTodasAsync(cancellationToken);
        if (ordens.Any(os => os.VeiculoId == id))
            throw new DominioException("Não é possível excluir um veículo com ordens de serviço vinculadas.");

        veiculoRepository.Remover(veiculo);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
