using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.Pecas;

public class AtualizarPecaUseCase(IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
{
    public async Task<PecaResponse> ExecutarAsync(Guid id, AtualizarPecaRequest request,
        CancellationToken cancellationToken = default)
    {
        var peca = await pecaRepository.ObterPorIdAsync(id, cancellationToken);
        if (peca == null)
            throw new DominioException("Peça não encontrada no catálogo.");

        peca.AlterarNome(request.Nome);
        peca.AlterarPreco(request.Preco);

        await pecaRepository.AtualizarAsync(peca, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new PecaResponse
        {
            Id = peca.Id,
            Nome = peca.Nome,
            Preco = peca.Preco,
            QuantidadeEstoque = peca.QuantidadeEstoque
        };
    }
}
