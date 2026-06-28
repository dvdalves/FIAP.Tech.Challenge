using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.Servicos;

public class ExcluirServicoUseCase(
    IServicoRepository servicoRepository,
    IOrdemServicoRepository ordemServicoRepository,
    IUnitOfWork unitOfWork)
{
    public async Task ExecutarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var servico = await servicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (servico == null)
            throw new DominioException("Serviço não encontrado.");

        var ordens = await ordemServicoRepository.ObterTodasAsync(cancellationToken);
        if (ordens.Any(os => os.Itens.Any(item => item.PecaId == null && item.Descricao == servico.Nome)))
            throw new DominioException("Não é possível excluir um serviço associado a ordens de serviço.");

        servicoRepository.Remover(servico);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
