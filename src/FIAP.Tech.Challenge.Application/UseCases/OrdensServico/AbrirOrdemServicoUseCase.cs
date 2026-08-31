using FIAP.Tech.Challenge.Application.DTOs.Responses;
using FIAP.Tech.Challenge.Application.Mappings;
using FIAP.Tech.Challenge.Domain;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.Exceptions;
using FIAP.Tech.Challenge.Domain.Services;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class AbrirOrdemServicoUseCase(
    IClienteRepository clienteRepository,
    IVeiculoRepository veiculoRepository,
    IOrdemServicoRepository ordemServicoRepository,
    IPecaRepository pecaRepository,
    IServicoRepository servicoRepository,
    IServicoNotificacao servicoNotificacao,
    IUnitOfWork unitOfWork)
{
    public async Task<OrdemServicoResponse> ExecutarAsync(AbrirOrdemServicoRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar se o cliente existe
        var cliente = await clienteRepository.ObterPorIdAsync(request.ClienteId, cancellationToken);
        if (cliente == null)
            throw new DominioException("Cliente não encontrado.");

        // 2. Validar se o veículo existe
        var veiculo = await veiculoRepository.ObterPorIdAsync(request.VeiculoId, cancellationToken);
        if (veiculo == null)
            throw new DominioException("Veículo não encontrado.");

        // 3. Validar se o veículo pertence a este cliente
        if (veiculo.ClienteId != cliente.Id)
            throw new DominioException("Este veículo não pertence ao cliente informado.");

        // 4. Criar OS
        var os = new OrdemServico(
            Guid.NewGuid(),
            cliente.Id,
            veiculo.Id,
            request.DescricaoProblema
        );

        // 5. Inclusão de peças e serviços se fornecidos na abertura
        var possuiItens = (request.ItensPeca != null && request.ItensPeca.Count > 0) ||
                          (request.ItensServico != null && request.ItensServico.Count > 0);

        if (possuiItens)
        {
            os.AtualizarStatus(StatusOrdemServico.EmDiagnostico);

            if (request.ItensPeca != null)
            {
                foreach (var itemPeca in request.ItensPeca)
                {
                    var peca = await pecaRepository.ObterPorIdAsync(itemPeca.PecaId, cancellationToken);
                    if (peca == null)
                        throw new DominioException($"Peça com ID '{itemPeca.PecaId}' não cadastrada no catálogo.");

                    if (peca.QuantidadeEstoque < itemPeca.Quantidade)
                        throw new DominioException($"Estoque insuficiente para a peça '{peca.Nome}'. Disponível: {peca.QuantidadeEstoque}, Solicitado: {itemPeca.Quantidade}.");

                    os.AdicionarItem(peca.Id, peca.Nome, itemPeca.Quantidade, peca.Preco, 0);
                }
            }

            if (request.ItensServico != null)
            {
                foreach (var itemServico in request.ItensServico)
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
            }

            os.FinalizarDiagnostico();
        }

        await ordemServicoRepository.AdicionarAsync(os, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        // Notificar cliente por e-mail sobre abertura da OS
        await servicoNotificacao.NotificarAtualizacaoStatusAsync(
            os.Id,
            cliente.Nome,
            cliente.Email,
            StatusOrdemServico.Recebida,
            os.Status,
            $"Ordem de serviço aberta com sucesso. Problema relatado: {os.DescricaoProblema}",
            cancellationToken);

        return os.ParaResponse();
    }
}