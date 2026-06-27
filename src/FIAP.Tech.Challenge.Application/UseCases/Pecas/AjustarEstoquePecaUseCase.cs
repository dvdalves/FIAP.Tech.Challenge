using System;
using System.Threading;
using System.Threading.Tasks;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.Pecas;

public class AjustarEstoquePecaUseCase(IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
{
    public async Task ExecutarAsync(Guid pecaId, int novaQuantidade, CancellationToken cancellationToken = default)
    {
        var peca = await pecaRepository.ObterPorIdAsync(pecaId, cancellationToken);
        if (peca == null)
            throw new DominioException("Peça não encontrada no estoque.");

        // Chama método rico do domínio
        peca.AjustarEstoque(novaQuantidade);

        await pecaRepository.AtualizarAsync(peca, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
