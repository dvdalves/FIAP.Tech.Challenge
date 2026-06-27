using System;
using System.Threading;
using System.Threading.Tasks;
using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class AtualizarStatusOSUseCase
{
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarStatusOSUseCase(
        IOrdemServicoRepository ordemServicoRepository,
        IUnitOfWork unitOfWork)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrdemServicoResponse> ExecutarAsync(Guid id, StatusOrdemServico novoStatus, decimal? valorOrcamento = null, CancellationToken cancellationToken = default)
    {
        var os = await _ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (os == null)
            throw new DominioException("Ordem de serviço não encontrada.");

        // Se estiver em Diagnóstico e o orçamento for informado, define o valor antes da transição para Aguardando Aprovação
        if (os.Status == StatusOrdemServico.EmDiagnostico && valorOrcamento.HasValue && valorOrcamento.Value > 0)
        {
            os.DefinirOrcamento(valorOrcamento.Value);
        }

        // Executa a transição de status validando as regras do domínio
        os.AtualizarStatus(novoStatus);

        await _ordemServicoRepository.AtualizarAsync(os, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return os.ParaResponse();
    }
}
