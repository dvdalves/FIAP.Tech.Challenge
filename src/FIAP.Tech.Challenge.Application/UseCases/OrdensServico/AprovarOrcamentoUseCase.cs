using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.Services;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class AprovarOrcamentoUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IPecaRepository pecaRepository,
    IClienteRepository clienteRepository,
    IServicoNotificacao servicoNotificacao,
    IUnitOfWork unitOfWork)
{
    public async Task<OrdemServicoResponse> ExecutarAsync(Guid osId, CancellationToken cancellationToken = default)
    {
        // 1. Carregar a Ordem de Serviço com os itens
        var os = await ordemServicoRepository.ObterPorIdAsync(osId, cancellationToken);
        if (os == null)
            throw new DominioException("Ordem de serviço não encontrada.");

        var statusAnterior = os.Status;

        // 2. Transicionar status para Em Execução
        os.AtualizarStatus(StatusOrdemServico.EmExecucao);

        // 3. Deduzir o estoque das peças de forma transacional
        foreach (var item in os.Itens)
            if (item.PecaId.HasValue)
            {
                var peca = await pecaRepository.ObterPorIdAsync(item.PecaId.Value, cancellationToken);
                if (peca == null)
                    throw new DominioException(
                        $"Peça '{item.Descricao}' associada ao orçamento não existe no catálogo.");

                // Método de negócio rico da entidade Peca que lança exceção se saldo for insuficiente
                peca.DeduzirEstoque(item.Quantidade);

                await pecaRepository.AtualizarAsync(peca, cancellationToken);
            }

        // 4. Salvar e confirmar transação única
        await ordemServicoRepository.AtualizarAsync(os, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        // 5. Notificar cliente por e-mail
        var cliente = await clienteRepository.ObterPorIdAsync(os.ClienteId, cancellationToken);
        if (cliente != null)
        {
            await servicoNotificacao.NotificarAtualizacaoStatusAsync(
                os.Id,
                cliente.Nome,
                cliente.Email,
                statusAnterior,
                StatusOrdemServico.EmExecucao,
                "Orçamento aprovado pelo cliente com sucesso. Início da execução dos reparos mecânicos.",
                cancellationToken);
        }

        return os.ParaResponse();
    }
}