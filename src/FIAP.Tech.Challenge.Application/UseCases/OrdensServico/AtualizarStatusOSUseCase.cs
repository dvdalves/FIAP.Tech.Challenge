using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.Services;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class AtualizarStatusOSUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IClienteRepository clienteRepository,
    IServicoNotificacao servicoNotificacao,
    IUnitOfWork unitOfWork)
{
    public async Task<OrdemServicoResponse> ExecutarAsync(Guid id, StatusOrdemServico novoStatus,
        CancellationToken cancellationToken = default)
    {
        var os = await ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);
        if (os == null)
            throw new DominioException("Ordem de serviço não encontrada.");

        var statusAnterior = os.Status;

        // Executa a transição de status validando as regras do domínio
        os.AtualizarStatus(novoStatus);

        await ordemServicoRepository.AtualizarAsync(os, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        // Notificação por e-mail ao cliente
        var cliente = await clienteRepository.ObterPorIdAsync(os.ClienteId, cancellationToken);
        if (cliente != null)
        {
            await servicoNotificacao.NotificarAtualizacaoStatusAsync(
                os.Id,
                cliente.Nome,
                cliente.Email,
                statusAnterior,
                novoStatus,
                $"Status atualizado para: {PerfilMapping.ObterDescricaoStatus(novoStatus)}",
                cancellationToken);
        }

        return os.ParaResponse();
    }
}