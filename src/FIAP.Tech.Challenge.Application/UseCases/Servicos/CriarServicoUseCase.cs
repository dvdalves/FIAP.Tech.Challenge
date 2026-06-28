using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.Servicos;

public class CriarServicoUseCase(
    IServicoRepository servicoRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<ServicoResponse> ExecutarAsync(CriarServicoRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new DominioException("Nome do serviço é obrigatório.");

        if (request.PrecoMaoDeObra < 0)
            throw new DominioException("O preço do serviço não pode ser negativo.");

        var servico = new Servico(
            Guid.NewGuid(),
            request.Nome,
            request.PrecoMaoDeObra
        );

        await servicoRepository.AdicionarAsync(servico, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new ServicoResponse
        {
            Id = servico.Id,
            Nome = servico.Nome,
            PrecoMaoDeObra = servico.PrecoMaoDeObra
        };
    }
}
