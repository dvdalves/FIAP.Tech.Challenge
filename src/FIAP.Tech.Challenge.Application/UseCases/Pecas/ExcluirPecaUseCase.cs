using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.Pecas;

public class ExcluirPecaUseCase(
    IPecaRepository pecaRepository,
    IOrdemServicoRepository ordemServicoRepository,
    IUnitOfWork unitOfWork)
{
    public async Task ExecutarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var peca = await pecaRepository.ObterPorIdAsync(id, cancellationToken);
        if (peca == null)
            throw new DominioException("Peça não encontrada no catálogo.");

        var ordens = await ordemServicoRepository.ObterTodasAsync(cancellationToken);
        if (ordens.Any(os => os.Itens.Any(item => item.PecaId == id)))
            throw new DominioException("Não é possível excluir uma peça associada a ordens de serviço.");

        pecaRepository.Remover(peca);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
