using FIAP.Tech.Challenge.Application.DTOs.Requests;
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
using FIAP.Tech.Challenge.Domain.ValueObjects;

namespace FIAP.Tech.Challenge.Application.UseCases.OrdensServico;

public class CriarOrdemServicoUseCase(
    IClienteRepository clienteRepository,
    IVeiculoRepository veiculoRepository,
    IOrdemServicoRepository ordemServicoRepository,
    IPecaRepository pecaRepository,
    IServicoRepository servicoRepository,
    IServicoNotificacao servicoNotificacao,
    IUnitOfWork unitOfWork)
{
    public async Task<OrdemServicoResponse> ExecutarAsync(CriarOrdemServicoRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar e Criar Objetos de Valor
        var cpf = new Cpf(request.ClienteCpf);
        var placa = new Placa(request.VeiculoPlaca);

        // 2. Obter ou Criar Cliente
        var cliente = await clienteRepository.ObterPorCpfAsync(cpf, cancellationToken);
        if (cliente == null)
        {
            cliente = new Cliente(
                Guid.NewGuid(),
                request.ClienteNome,
                cpf,
                request.ClienteEmail,
                request.ClienteTelefone
            );
            await clienteRepository.AdicionarAsync(cliente, cancellationToken);
        }

        // 3. Obter ou Criar Veículo
        var veiculo = await veiculoRepository.ObterPorPlacaAsync(placa, cancellationToken);
        if (veiculo == null)
        {
            veiculo = new Veiculo(
                Guid.NewGuid(),
                placa,
                request.VeiculoMarca,
                request.VeiculoModelo,
                request.VeiculoAno,
                cliente.Id
            );
            await veiculoRepository.AdicionarAsync(veiculo, cancellationToken);
        }
        else if (veiculo.ClienteId != cliente.Id)
        {
            throw new DominioException("Este veículo já está cadastrado para outro cliente.");
        }

        // 4. Criar Ordem de Serviço
        var ordemServico = new OrdemServico(
            Guid.NewGuid(),
            cliente.Id,
            veiculo.Id,
            request.DescricaoProblema
        );

        // 5. Inclusão de peças e serviços se fornecidos na criação
        var possuiItens = (request.ItensPeca != null && request.ItensPeca.Count > 0) ||
                          (request.ItensServico != null && request.ItensServico.Count > 0);

        if (possuiItens)
        {
            ordemServico.AtualizarStatus(StatusOrdemServico.EmDiagnostico);

            if (request.ItensPeca != null)
            {
                foreach (var itemPeca in request.ItensPeca)
                {
                    var peca = await pecaRepository.ObterPorIdAsync(itemPeca.PecaId, cancellationToken);
                    if (peca == null)
                        throw new DominioException($"Peça com ID '{itemPeca.PecaId}' não cadastrada no catálogo.");

                    if (peca.QuantidadeEstoque < itemPeca.Quantidade)
                        throw new DominioException($"Estoque insuficiente para a peça '{peca.Nome}'. Disponível: {peca.QuantidadeEstoque}, Solicitado: {itemPeca.Quantidade}.");

                    ordemServico.AdicionarItem(peca.Id, peca.Nome, itemPeca.Quantidade, peca.Preco, 0);
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

                    ordemServico.AdicionarItem(null, descricao, 1, 0, valorMaoDeObra);
                }
            }

            ordemServico.FinalizarDiagnostico();
        }

        await ordemServicoRepository.AdicionarAsync(ordemServico, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        // Notificar cliente por e-mail sobre criação da OS
        await servicoNotificacao.NotificarAtualizacaoStatusAsync(
            ordemServico.Id,
            cliente.Nome,
            cliente.Email,
            StatusOrdemServico.Recebida,
            ordemServico.Status,
            $"Ordem de serviço criada com sucesso. Problema relatado: {ordemServico.DescricaoProblema}",
            cancellationToken);

        return ordemServico.ParaResponse();
    }
}