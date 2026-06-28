using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.Servicos;

public class AtualizarServicoUseCase(IServicoRepository servicoRepository, IUnitOfWork unitOfWork)
{
    public async Task<ServicoResponse> ExecutarAsync(Guid id, AtualizarServicoRequest request,
        CancellationToken cancellationToken = default)
    {
        var servico = await servicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (servico == null)
            throw new DominioException("Serviço não encontrado.");

        servico.AlterarNome(request.Nome);
        servico.AlterarPreco(request.PrecoMaoDeObra);

        await servicoRepository.AtualizarAsync(servico, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return new ServicoResponse
        {
            Id = servico.Id,
            Nome = servico.Nome,
            PrecoMaoDeObra = servico.PrecoMaoDeObra
        };
    }
}
