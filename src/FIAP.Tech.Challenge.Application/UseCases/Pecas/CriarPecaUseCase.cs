using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;

namespace FIAP.Tech.Challenge.Application.UseCases.Pecas;

public class CriarPecaUseCase(IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
{
    public async Task<PecaResponse> ExecutarAsync(AdicionarPecaRequest request, CancellationToken cancellationToken = default)
    {
        var peca = new Peca(
            Guid.NewGuid(),
            request.Nome,
            request.Preco,
            request.QuantidadeEstoque
        );

        await pecaRepository.AdicionarAsync(peca, cancellationToken);
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
