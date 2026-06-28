using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class LancarItensOSUseCase(
    IOrdemServicoRepository ordemServicoRepository,
    IPecaRepository pecaRepository,
    IServicoRepository servicoRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<OrdemServicoResponse> ExecutarAsync(Guid osId, LancarItensOSRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Carregar a Ordem de Serviço
        var os = await ordemServicoRepository.ObterPorIdAsync(osId, cancellationToken);
        if (os == null)
            throw new DominioException("Ordem de serviço não encontrada.");

        // Se estiver em Recebida, move para EmDiagnostico
        if (os.Status == StatusOrdemServico.Recebida) os.AtualizarStatus(StatusOrdemServico.EmDiagnostico);

        // 2. Adicionar as peças solicitadas
        foreach (var itemPeca in request.Pecas)
        {
            var peca = await pecaRepository.ObterPorIdAsync(itemPeca.PecaId, cancellationToken);
            if (peca == null)
                throw new DominioException($"Peça com ID '{itemPeca.PecaId}' não cadastrada no catálogo.");

            // Validar prévia de estoque antes de adicionar ao orçamento
            if (peca.QuantidadeEstoque < itemPeca.Quantidade)
                throw new DominioException(
                    $"Estoque insuficiente para a peça '{peca.Nome}'. Disponível: {peca.QuantidadeEstoque}, Solicitado: {itemPeca.Quantidade}.");

            os.AdicionarItem(peca.Id, peca.Nome, itemPeca.Quantidade, peca.Preco, 0);
        }

        // 3. Adicionar as mãos de obra (serviços)
        foreach (var itemServico in request.Servicos)
        {
            var descricao = itemServico.Descricao;
            var valorMaoDeObra = itemServico.ValorMaoDeObra;

            if (itemServico.ServicoId.HasValue && itemServico.ServicoId.Value != Guid.Empty)
            {
                var servico = await servicoRepository.ObterPorIdAsync(itemServico.ServicoId.Value, cancellationToken);
                if (servico == null)
                    throw new DominioException($"Serviço com ID '{itemServico.ServicoId}' não cadastrado no catálogo.");

                descricao = servico.Nome;
                valorMaoDeObra = servico.PrecoMaoDeObra;
            }

            os.AdicionarItem(null, descricao, 1, 0, valorMaoDeObra);
        }

        // 4. Finalizar o diagnóstico (recalcula orçamento e transiciona para AguardandoAprovacao)
        os.FinalizarDiagnostico();

        // 5. Persistir no banco de dados
        await ordemServicoRepository.AtualizarAsync(os, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return os.ParaResponse();
    }
}